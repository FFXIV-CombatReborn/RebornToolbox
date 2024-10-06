using Dalamud;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.Commands;
using ECommons.DalamudServices;
using RebornToolbox.Common;
using RebornToolbox.Features.ChocoboRacing;
using RebornToolbox.Features.InvSort;
using RebornToolbox.Features.MBShoppingList;

namespace RebornToolbox;

public class Plugin : IDalamudPlugin
{
    public static Configuration Configuration { get; set; }
    public MBShoppingList MBShoppingList;
    //public InvSort InventorySort;
    public ChocoboRacing ChocoboRacing;

    public WindowSystem WindowSystem;
    public MainWindow MainWindow;
    public MBShoppingList_UI ShoppingListUI;
    public ChocoboRacing_UI ChocoboRacingUI;

    public Plugin(IDalamudPluginInterface plugin)
    {
        ECommonsMain.Init(plugin, this, Module.DalamudReflector);
        Configuration = Configuration.LoadConfig();
        MBShoppingList = new MBShoppingList();
        //InventorySort = new InvSort();
        ChocoboRacing = new ChocoboRacing();

        WindowSystem = new WindowSystem("RebornToolbox");
        MainWindow = new MainWindow(this);
        ShoppingListUI = new MBShoppingList_UI(MBShoppingList);
        ChocoboRacingUI = new ChocoboRacing_UI(ChocoboRacing);

        WindowSystem.AddWindow(ShoppingListUI);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ChocoboRacingUI);

        Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleMainWindow;
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;
    }

    public void ToggleSweepWindow()
    {
        if (!Configuration.ShoppingListConfig.Enabled)
        {
            Svc.Chat.PrintError($"[Reborn Toolbox] Supermarket Sweep is disabled.");
            return;
        }
        ShoppingListUI.IsOpen = !ShoppingListUI.IsOpen;
    }

    public void ToggleChocoboRacingWindow()
    {
        if (!Configuration.ChocoboRacingConfig.Enabled)
        {
            Svc.Chat.PrintError($"[Reborn Toolbox] Feather to the Metal is disabled.");
            return;
        }
        ChocoboRacingUI.IsOpen = !ChocoboRacingUI.IsOpen;
    }

    public void ToggleMainWindow()
    {
        MainWindow.IsOpen = !MainWindow.IsOpen;
    }

    [Cmd("/rbtoolbox", "Open Config Window")]
    public void OnCommand(string command, string arguments)
    {
        if (string.Equals(arguments, "shop"))
        {
            ToggleSweepWindow();
        }
        else if (string.Equals(arguments, "expert"))
        {
            Plugin.Configuration.ExpertMode = !Plugin.Configuration.ExpertMode;
            Svc.Chat.Print($"[Reborn Toolbox] ExpertMode: {Plugin.Configuration.ExpertMode}");
            Plugin.Configuration.SaveConfig();
        }

        else if (string.Equals(arguments, "race"))
        {
            ToggleChocoboRacingWindow();
        }
        else
        {
            ToggleMainWindow();
        }
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
    }
}