using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace RebornToolbox.Features.MBShoppingList.Models;

public class ShoppingListItem
{
    public ShoppingListItem(Item item, int quantity)
    {
        ItemId = item.RowId;
        Quantity = quantity;
        Task.Run(GetMarketDataResponseAsync);
    }

    public ShoppingListItem()
    {
        Task.Run(GetMarketDataResponseAsync);
    }

    public ulong ItemId;

    [Newtonsoft.Json.JsonIgnore]
    public Item ItemRecord => MBShoppingList.AllItems.First(x => x.RowId == ItemId);

    public bool IsMarketable => MBShoppingList.MarketableItems.Contains(ItemRecord);
    public string Name => ItemRecord.Name;
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
    [Newtonsoft.Json.JsonIgnore]
    private int _retries = 0;

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
                }
                else
                {
                    Svc.Log.Warning($"Unable to get market data response from universalis: {responseString}");
                }
            }
            catch
            {
                _retries++;
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}