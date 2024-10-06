using Dalamud.Interface.Windowing;
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
        }
    }
}