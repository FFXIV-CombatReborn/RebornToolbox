using Dalamud.Interface.Windowing;
using ECommons.Automation;
using ECommons.Commands;
using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using RebornToolbox.Features.MBShoppingList.Models;

namespace RebornToolbox.Features.MBShoppingList;

public class MBShoppingList
{
    public static List<Item> AllItems;

    public static List<Item> MarketableItems;

    public List<ShoppingListItem> WantedItems = [];

    public Chat Chat;

    public MBShoppingList()
    {
        AllItems = Svc.Data.GameData.GetExcelSheet<Item>()!.ToList();
        MarketableItems = AllItems.Where(i => i.ItemSearchCategory.Row != 0).ToList();

        Chat = new Chat();
        LoadList();
    }

    public void SaveList()
    {
        var path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "shoppinglist.json");
        var json = JsonConvert.SerializeObject(WantedItems, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public void LoadList()
    {
        var path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "shoppinglist.json");
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var shoppingList = JsonConvert.DeserializeObject<List<ShoppingListItem>>(json);
            if (shoppingList != null)
            {
                WantedItems = shoppingList;
            }
        }
    }
}