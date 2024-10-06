﻿using Dalamud;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Commands;
using ECommons.DalamudServices;
using RebornToolbox.Common;
using RebornToolbox.Features.MBShoppingList;

namespace RebornToolbox;

public class Plugin : IDalamudPlugin
{
    public static Configuration Configuration { get; set; }
    public MBShoppingList MBShoppingList;

    public WindowSystem WindowSystem;
    public MainWindow MainWindow;
    public MBShoppingList_UI ShoppingListUI;

    public Plugin(IDalamudPluginInterface plugin)
    {
        ECommonsMain.Init(plugin, this);
        Configuration = Configuration.LoadConfig();
        MBShoppingList = new MBShoppingList();

        WindowSystem = new WindowSystem("RebornToolbox");
        MainWindow = new MainWindow(this);
        ShoppingListUI = new MBShoppingList_UI(MBShoppingList);

        WindowSystem.AddWindow(ShoppingListUI);
        WindowSystem.AddWindow(MainWindow);

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