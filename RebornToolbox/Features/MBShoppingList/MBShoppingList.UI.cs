using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Style;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using RebornToolbox.Features.MBShoppingList.Models;

namespace RebornToolbox.Features.MBShoppingList;

public class MBShoppingList_UI : Window
{
    private MBShoppingList _manager;
    private FileDialogManager _fileDialogManager;

    public MBShoppingList_UI(MBShoppingList manager) : base("Supermarket Sweep", ImGuiWindowFlags.None, false)
    {
        _manager = manager;
        _fileDialogManager = new FileDialogManager();
    }

    public override void Draw()
    {
        DrawItemAdd();
        ImGui.SameLine();
        if (ImGui.Button("Import MakePlace List"))
        {
            SelectFile();
        }
        RenderRegionTypeComboBox();
        var maxResults = Plugin.Configuration.ShoppingListConfig.MaxResults;
        if (ImGui.InputInt("Max Search Results", ref maxResults))
        {
            Plugin.Configuration.ShoppingListConfig.MaxResults = maxResults;
            Plugin.Configuration.SaveConfig();
        }
        if (ImGui.Button("Refresh Search Results"))
        {
            foreach (var item in _manager.WantedItems)
            {
                item.ClearDataResponse();
                Task.Run(item.GetMarketDataResponseAsync);
            }
        }
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();
        foreach (var item in _manager.WantedItems)
        {
            DrawWantedItem(item);
        }

        if (_itemsToRemove.Any())
        {
            foreach (var item in _itemsToRemove)
            {
                _manager.WantedItems.Remove(item);
            }
            _itemsToRemove.Clear();
        }
        _fileDialogManager.Draw();
    }

    private List<ShoppingListItem> _itemsToRemove = new();

    private void DrawWantedItem(ShoppingListItem item)
    {
        var extraText = item.InventoryCount >= item.Quantity ? "(Completed)" : string.Empty;
        if (ImGui.CollapsingHeader($"{item.Name} {extraText}"))
        {
            var quantity = item.Quantity;
            if (ImGui.InputInt("Needed Quantity", ref quantity))
            {
                item.Quantity = quantity;
                _manager.SaveList();
            }

            var color = item.InventoryCount >= item.Quantity ? ImGuiColors.HealerGreen : ImGuiColors.DalamudWhite;
            ImGui.TextColored(color, $"Amount in Inventory: {item.InventoryCount}");

            if (ImGui.Button($"Remove Item##{item.ItemId}") && (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl)))
            {
                _itemsToRemove.Add(item);
            }
            ImGuiEx.Tooltip("Hold CTRL while clicking to remove item from list");

            DrawItemSearch(item);

            if (item.MarketDataResponse == null)
            {
                if (ImGui.Button("Pull Market Data"))
                {
                    Task.Run(item.GetMarketDataResponseAsync);
                }
            }
            else
            {
                ImGui.Separator();

                foreach (var world in item.WorldListings)
                {
                    if (ImGui.Button($"{world.WorldName}"))
                    {
                        _manager.Chat.ExecuteCommand($"/li {world.WorldName}");
                    }
                    ImGuiEx.Tooltip("Travel using LifeStream");
                    ImGui.Text($"Number of Listings: {world.Count}");
                    ImGui.Text($"Lowest Price: {world.LowestPrice}");
                    ImGui.Separator();
                }
            }
        }
    }

    private unsafe void DrawItemSearch(ShoppingListItem item)
    {
        AddonItemSearch* addonItemSearch = (AddonItemSearch*)Svc.GameGui.GetAddonByName("ItemSearch");
        if (addonItemSearch == null)
        {
            ImGui.Text("MarketBoard not opened");
            ImGuiEx.Tooltip("Open the market board window to automatically search for items");
        }
        else
        {
            if (ImGui.Button($"Search Marketboard for Item##{item.ItemId}"))
            {
                addonItemSearch->SearchTextInput->SetText(item.Name);
                addonItemSearch->RunSearch();
            }
        }
    }

    private void SelectFile()
    {
        void Callback(bool finished, string path)
        {
            if (finished && !string.IsNullOrEmpty(path))
            {
                string text = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ExtractItemsFromMakePlaceList(text, "Furniture", "Dyes");
                    ExtractItemsFromMakePlaceList(text, "Dyes", "Furniture (With Dye)");
                    _manager.SaveList();
                }
            }
        }

        _fileDialogManager.OpenFileDialog("Select MakePlace List File", "MakePlace List Files{.txt}", Callback);
    }

    private string _searchTerm = string.Empty;
    private Item? _selectedItem;
    private void DrawItemAdd()
    {
        ImGui.InputText("##searchBar", ref _searchTerm, 100);

        if (!string.IsNullOrEmpty(_searchTerm) && _selectedItem is null)
        {
            var matchingItems = MBShoppingList.MarketableItems.Where(item => item.Name.ToString().Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));

            if (matchingItems.Any())
            {
                ImGui.BeginChild($"ItemList", new Vector2(0, 150), true);
                foreach (var item in matchingItems)
                {
                    if (ImGui.Selectable(item.Name))
                    {
                        _selectedItem = item;
                        _searchTerm = item.Name.ToString();
                    }
                }
                ImGui.EndChild();
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Add"))
        {
            if (_selectedItem is null)
            {
                Svc.Log.Warning("No item to add to shopping list");
                return;
            }
            var shoppingListItem = new ShoppingListItem(_selectedItem, 1);
            _manager.WantedItems.Add(shoppingListItem);
            _searchTerm = string.Empty;
            _selectedItem = null;
            _manager.SaveList();
            Svc.Log.Debug($"Added shopping list item: {shoppingListItem.Name}");
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear"))
        {
            _selectedItem = null;
            _searchTerm = string.Empty;
        }
    }

    private static readonly string[] RegionNames = ["North America", "Europe", "Japan", "Oceania"];
    public void RenderRegionTypeComboBox()
    {
        int currentRegionIndex = (int)Plugin.Configuration.ShoppingListConfig.ShoppingRegion;

        if (ImGui.Combo("Select Region", ref currentRegionIndex, RegionNames, RegionNames.Length))
        {
            Plugin.Configuration.ShoppingListConfig.ShoppingRegion = (RegionType)currentRegionIndex;

            Plugin.Configuration.SaveConfig();
        }
    }

    private unsafe void DrawMBButton(ShoppingListItem item)
    {
        var mbAddon = (AddonItemSearch*)Svc.GameGui.GetAddonByName("ItemSearch");
        if (mbAddon == null)
            return;

        ImGui.SameLine();

        if (ImGui.Button($"Search##{item.ItemId}"))
        {
            mbAddon->SearchTextInput->SetText(item.Name);
            mbAddon->RunSearch();
        }
    }

    void ExtractItemsFromMakePlaceList(string text, string startSection, string endSection)
    {
        // Find the start index of the section
        int startIndex = text.IndexOf(startSection);
        if (startIndex == -1)
        {
            Svc.Log.Error($"Section '{startSection}' not found.");
            return;
        }

        // Find the end index of the section
        int endIndex = text.IndexOf(endSection, startIndex);
        if (endIndex == -1)
        {
            endIndex = text.Length; // If end section not found, read till end
        }

        // Extract the section text
        string sectionText = text.Substring(startIndex, endIndex - startIndex);

        // Split the section into lines
        string[] lines = sectionText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Regular expression to match lines with format 'Item Name: Quantity'
        Regex regex = new Regex(@"^\s*(.+?):\s*(\d+)\s*$");

        // Iterate over each line and extract item name and quantity
        foreach (string line in lines)
        {
            Match match = regex.Match(line);
            if (match.Success)
            {
                string itemName = match.Groups[1].Value.Trim();
                if (string.Equals(startSection, "Dyes", StringComparison.OrdinalIgnoreCase))
                    itemName += " Dye";
                int quantity = int.Parse(match.Groups[2].Value.Trim());

                var item = MBShoppingList.AllItems.FirstOrDefault(i =>
                    string.Equals(i.Name, itemName, StringComparison.OrdinalIgnoreCase));
                if (item == null && string.Equals(startSection, "Dyes", StringComparison.OrdinalIgnoreCase))
                {
                    item = MBShoppingList.AllItems.FirstOrDefault(i =>
                        string.Equals(i.Name, $"General-purpose {itemName}", StringComparison.OrdinalIgnoreCase));
                }
                if (item == null)
                {
                    Svc.Log.Warning($"Item '{itemName}' does not exist.");
                    continue;
                }

                var existingItem = _manager.WantedItems.FirstOrDefault(i => i.ItemId == item.RowId);

                // Add or update the item in the dictionary
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    _manager.WantedItems.Add(new ShoppingListItem(item, quantity));
                }
            }
        }
    }
}