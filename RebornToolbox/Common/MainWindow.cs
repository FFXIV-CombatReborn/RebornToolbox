using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using ImGuiNET;

namespace RebornToolbox.Common;

public class MainWindow : Window
{
    private Plugin _plugin;

    public MainWindow(Plugin plugin) : base("Reborn Toolbox", ImGuiWindowFlags.None, false)
    {
        _plugin = plugin;
    }

    public override void Draw()
    {
        var sweepEnabled = Plugin.Configuration.ShoppingListConfig.Enabled;
        if (ImGui.Checkbox("Enable Supermarket Sweep", ref sweepEnabled))
        {
            Plugin.Configuration.ShoppingListConfig.Enabled = sweepEnabled;
            Plugin.Configuration.SaveConfig();
        }

        ImGui.Indent();
        if (!sweepEnabled)
        {
            ImGui.TextWrapped(
                "SuperMarket sweep is a feature designed to make it easy to pick up whatever you need from the market board. Designed specifically to help with acquiring furniture for MakePlace builds, it has been expanded to be useful for any type of item you might need to acquire in bulk.");
        }
        else
        {
            if (ImGui.Button("Open Supermarket Sweep Window"))
            {
                _plugin.ToggleSweepWindow();
            }
            ImGuiEx.Tooltip("Also valid: '/rbtoolbox shop'");

            var moveEnabled = Plugin.Configuration.ShoppingListConfig.UseVnavPathing;
            if (ImGui.Checkbox("Automatically move to marketboard after teleporting", ref moveEnabled))
            {
                Plugin.Configuration.ShoppingListConfig.UseVnavPathing = moveEnabled;
                Plugin.Configuration.SaveConfig();
            }

            ImGuiEx.Tooltip(
                "Use VNavmesh to move to the nearest marketboard automatically after teleporting to a new server. (only Limsa and Grid are supported)");

            var inventoryCheck = Plugin.Configuration.ShoppingListConfig.RemoveQuantityAutomatically;
            if (ImGui.Checkbox("Remove Quantity Automatically", ref inventoryCheck))
            {
                Plugin.Configuration.ShoppingListConfig.RemoveQuantityAutomatically = inventoryCheck;
                Plugin.Configuration.SaveConfig();
            }
            ImGuiEx.Tooltip("Automatically subtract from needed quantity when items are added to your inventory.");

            var allChars = Plugin.Configuration.ShoppingListConfig.AllCharactersInventory;
            if (ImGui.Checkbox("Consider Inventory Counts from All Characters", ref allChars))
            {
                Plugin.Configuration.ShoppingListConfig.AllCharactersInventory = allChars;
                Plugin.Configuration.SaveConfig();
            }
            ImGuiEx.Tooltip("Whether or not to pull items from every character's inventory using Allagan Tools");
        }

        ImGui.Unindent();

        var chocoboRacing = Plugin.Configuration.ChocoboRacingConfig.Enabled;
        if (ImGui.Checkbox("Enable Feather to the Metal", ref chocoboRacing))
        {
            Plugin.Configuration.ChocoboRacingConfig.Enabled = chocoboRacing;
            Plugin.Configuration.SaveConfig();
        }

        ImGui.Indent();
        if (!chocoboRacing)
        {
            ImGui.TextWrapped($"Automatically queue for chocobo races and (optionally) hold W while in the race to automatically run.");
        }
        else
        {
            if (ImGui.Button("Open Feather to the Metal Window"))
            {
                _plugin.ChocoboRacingUI.IsOpen = !_plugin.ChocoboRacingUI.IsOpen;
            }
            ImGuiEx.Tooltip("Also valid: '/rbtoolbox race'");

            var moveEnabled = Plugin.Configuration.ChocoboRacingConfig.AutoRun;
            if (ImGui.Checkbox("Automatically run inside of race", ref moveEnabled))
            {
                Plugin.Configuration.ChocoboRacingConfig.AutoRun = moveEnabled;
                Plugin.Configuration.SaveConfig();
            }

            var alwaysRun = Plugin.Configuration.ChocoboRacingConfig.AlwaysRun;
            if (ImGui.Checkbox("Always Run (Ignore stamina/lathered status)", ref alwaysRun))
            {
                Plugin.Configuration.ChocoboRacingConfig.AlwaysRun = alwaysRun;
                Plugin.Configuration.SaveConfig();
            }

            var stopAtMaxRank = Plugin.Configuration.ChocoboRacingConfig.StopAtMaxRank;
            if (ImGui.Checkbox("Stop at Rank 40", ref stopAtMaxRank))
            {
                Plugin.Configuration.ChocoboRacingConfig.StopAtMaxRank = stopAtMaxRank;
                Plugin.Configuration.SaveConfig();
            }
        }
        ImGui.Unindent();

        // var sortEnabled = Plugin.Configuration.InvSortConfig.Enabled;
        // if (ImGui.Checkbox("Automatically sort inventory", ref sortEnabled))
        // {
        //     Plugin.Configuration.InvSortConfig.Enabled = sortEnabled;
        //     Plugin.Configuration.SaveConfig();
        // }
        //
        // ImGui.Indent();
        // if (!sortEnabled)
        // {
        //     ImGui.TextWrapped($"Automatically sort your inventory whenever it updates");
        // }
        // else
        // {
        //     ImGui.Text("You can hold CTRL to temporarily disable");
        //     var minUpdate = Plugin.Configuration.InvSortConfig.MinUpdateInterval;
        //     if (ImGui.InputInt("Minimum Update Interval", ref minUpdate))
        //     {
        //         Plugin.Configuration.InvSortConfig.MinUpdateInterval = minUpdate;
        //         Plugin.Configuration.SaveConfig();
        //     }
        //
        //     ImGuiEx.Tooltip(
        //         "Don't sort if it has been less than this many second since the last sort (prevents server spam when you get a lot of updates)");
        // }
        //
        // ImGui.Unindent();
    }
}