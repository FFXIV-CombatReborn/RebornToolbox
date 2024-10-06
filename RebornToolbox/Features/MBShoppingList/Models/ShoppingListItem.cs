using System.Runtime.Serialization;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace RebornToolbox.Features.MBShoppingList.Models;

public class ShoppingListItem
{
    [Newtonsoft.Json.JsonIgnore]
    private Item _itemRecord;

    public ShoppingListItem(Item item, int quantity)
    {
        ItemId = item.RowId;
        Quantity = quantity;
        _itemRecord = item; // Cache the item here
        IsMarketable = MBShoppingList.MarketableItems.Contains(ItemRecord);
    }

    public ShoppingListItem() {}

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        // Initialize _itemRecord after deserialization
        _itemRecord = MBShoppingList.AllItems.FirstOrDefault(x => x.RowId == ItemId);
        if (_itemRecord == null)
        {
            // Handle the case where the item is not found
            Svc.Log.Error($"Item with ID {ItemId} not found in AllItems.");
        }
        else
        {
            IsMarketable = MBShoppingList.MarketableItems.Contains(_itemRecord);
        }
    }

    [Newtonsoft.Json.JsonIgnore]
    public Item ItemRecord => _itemRecord;

    [Newtonsoft.Json.JsonIgnore]
    public string Name => _itemRecord.Name;

    public uint ItemId { get; set; }

    public bool IsMarketable { get; private set; }
    public int Quantity { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public unsafe int InventoryCount
    {
        get
        {
            int count = 0;
            var manager = InventoryManager.Instance();
            if (manager == null)
                return count;
            foreach (var inv in InventoryTypes)
            {
                var container = manager->GetInventoryContainer(inv);
                if (container == null || container->Loaded == 0)
                    continue;
                for (int i = 0; i < container->Size; i++)
                {
                    var item = container->GetInventorySlot(i);
                    if (item == null || item->ItemId == 0 || item->ItemId != ItemId) continue;

                    count++;
                }
            }

            return count;
        }
    }
    [Newtonsoft.Json.JsonIgnore]
    private List<InventoryType> InventoryTypes
    {
        get
        {
            List<InventoryType> types = new List<InventoryType>()
            {
                InventoryType.Inventory1,
                InventoryType.Inventory2,
                InventoryType.Inventory3,
                InventoryType.Inventory4,
            };
            return types;
        }
    }
    [Newtonsoft.Json.JsonIgnore]
    public MarketDataListing? BestMarketListing => MarketDataResponse?.Listings.OrderBy(l => l.Total).FirstOrDefault();
    [Newtonsoft.Json.JsonIgnore]
    public MarketDataResponse? MarketDataResponse { get; private set; }

    [JsonIgnore]
    private Task? _marketDataTask;

    [JsonIgnore]
    private int _retries;

    [JsonIgnore]
    private bool _isFetchingData;

    [Newtonsoft.Json.JsonIgnore]
    public bool IsFetchingData
    {
        get
        {
            lock (this)
            {
                return _isFetchingData;
            }
        }
    }

    [Newtonsoft.Json.JsonIgnore]
    public int Retries
    {
        get
        {
            lock (this)
            {
                return _retries;
            }
        }
    }

    public List<WorldListing> WorldListings { get; private set; } = [];

    public class WorldListing
    {
        public string WorldName { get; set; }
        public int Count { get; set; }
        public long LowestPrice { get; set; }
        public List<MarketDataListing> Listings { get; set; } = new List<MarketDataListing>();
    }

    public void ClearDataResponse()
    {
        MarketDataResponse = null;
    }
    public async Task GetMarketDataResponseAsync()
    {
        if (!IsMarketable)
            return;

        Task existingTask;
        lock (this)
        {
            if (_marketDataTask != null && !_marketDataTask.IsCompleted)
            {
                existingTask = _marketDataTask;
            }
            else
            {
                _isFetchingData = true;
                _retries = 0;
                _marketDataTask = FetchMarketDataAsync();
                existingTask = _marketDataTask;
            }
        }

        await existingTask;
    }

    private async Task FetchMarketDataAsync()
    {
        while (_retries < 5 && MarketDataResponse == null)
        {
            Svc.Log.Debug($"GetMarketDataResponseAsync for item {Name}");
            try
            {
                using var client = new HttpClient();

                var maxResults = Plugin.Configuration.ShoppingListConfig.MaxResults;
                var responseString = await client
                    .GetStringAsync($"https://universalis.app/api/v2/{Plugin.Configuration.ShoppingListConfig.ShoppingRegion.ToUniversalisString()}/{ItemId}?listings={maxResults}&entries={maxResults}")
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(responseString))
                {
                    MarketDataResponse = JsonConvert.DeserializeObject<MarketDataResponse>(responseString);
                    WorldListings = MarketDataResponse!.Listings
                        .GroupBy(l => l.WorldName)
                        .Select(g => new WorldListing
                        {
                            WorldName = g.Key,
                            Count = g.Count(),
                            LowestPrice = g.Min(l => l.Total),
                            Listings = g.ToList()
                        })
                        .OrderBy(l => l.LowestPrice)
                        .ToList();
                    break; // Fetch successful
                }
                else
                {
                    Svc.Log.Warning($"Unable to get market data response from Universalis: {responseString}");
                    lock (this)
                    {
                        _retries++;
                    }
                    await Task.Delay(2000);
                }
            }
            catch
            {
                lock (this)
                {
                    _retries++;
                }
                await Task.Delay(2000);
            }
        }
        lock (this)
        {
            _isFetchingData = false;
        }
    }

}