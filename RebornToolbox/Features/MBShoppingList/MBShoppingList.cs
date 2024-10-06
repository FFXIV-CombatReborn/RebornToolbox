using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface.Windowing;
using ECommons;
using ECommons.Automation;
using ECommons.Automation.NeoTaskManager;
using ECommons.Commands;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using RebornToolbox.Features.MBShoppingList.Models;
using RebornToolbox.IPC;

namespace RebornToolbox.Features.MBShoppingList;

public class MBShoppingList
{
    public static List<Item> AllItems;

    public static List<Item> MarketableItems;

    public List<ShoppingListItem> WantedItems = [];

    public Chat Chat;

    public TaskManager TaskManager;

#if DEBUG
    static bool showDebug = true;
#else
    static bool showDebug = false;
#endif

    public TaskManagerConfiguration LifeStreamTaskConfig = new TaskManagerConfiguration(
        timeLimitMS: Plugin.Configuration.ShoppingListConfig.LifeStreamTimeout * 1000, showDebug: showDebug);

    public TaskManagerConfiguration DefaultTaskConfig =
        new TaskManagerConfiguration(timeLimitMS: 40000, showDebug: showDebug);

    public MBShoppingList()
    {
        AllItems = Svc.Data.GameData.GetExcelSheet<Item>()!.ToList();
        MarketableItems = AllItems.Where(i => i.ItemSearchCategory.Row != 0).ToList();

        Chat = new Chat();

        TaskManager = new TaskManager(DefaultTaskConfig);
        LoadList();

        Svc.GameInventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        if (!Plugin.Configuration.ShoppingListConfig.Enabled ||
            !Plugin.Configuration.ShoppingListConfig.RemoveQuantityAutomatically)
            return;

        bool updated = false;
        foreach (var e in events)
        {
            var item = WantedItems.FirstOrDefault(i => i.ItemId == e.Item.ItemId);

            if (item != null)
            {
                updated = true;
                Svc.Log.Debug($"Item {item.Name} has moved into inventory.");
                item.Quantity--;
                if (item.Quantity < 0)
                    item.Quantity = 0;
            }
        }
        if (updated)
            SaveList();
    }

    public void SaveList()
    {
        var path = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "shoppinglist.json");
        var json = JsonConvert.SerializeObject(WantedItems, Formatting.Indented);
        File.WriteAllText(path, json);
        Svc.Log.Verbose("Shopping list saved.");
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
                Svc.Log.Verbose("Shopping list loaded.");
            }
        }
    }

    public void QueueMoveToMarketboardTasks()
    {
        if (!Plugin.Configuration.ShoppingListConfig.UseVnavPathing)
            return;
        if (!VNavmesh_IPCSubscriber.IsEnabled)
        {
            Svc.Chat.PrintError($"[Reborn Toolbox] VNavmesh is required for automatic movement");
            return;
        }
        switch (Svc.ClientState.TerritoryType)
        {
            case 129:
                MoveToNearestMarketboard();
                return;
            // case 130:
            //     QueueUldahMoveToMarketboardTasks();
            //     return;
            case 132:
                QueueGridMoveToMarketboardTasks();
                return;
            default:
                Svc.Log.Error($"TerritoryType {Svc.ClientState.TerritoryType} is not supported.");
                return;
        }
    }

    private Vector3 uldahTransitionPosition = new Vector3(97.695f, 4f, 107.734f);

    private void QueueUldahMoveToMarketboardTasks()
    {
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Nav_IsReady());
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.SimpleMove_PathfindAndMoveTo(uldahTransitionPosition, false));
        TaskManager.EnqueueDelay(5000);
        TaskManager.Enqueue(() => !VNavmesh_IPCSubscriber.Path_IsRunning() && !VNavmesh_IPCSubscriber.Nav_PathfindInProgress());
        TaskManager.Enqueue(GenericHelpers.IsScreenReady);
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Nav_IsReady());
        TaskManager.Enqueue(MoveToNearestMarketboard);
    }

    private uint marketboardDataId = 2000402;

    private Vector3 oldGridTransitionPosition = new Vector3(11.458f, 1.275f, -15.702f);
    private Vector3 oldGridBowerHousePosition = new Vector3(141.558f, 13.571f, -97.028f);
    private void QueueGridMoveToMarketboardTasks()
    {
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Nav_IsReady());
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.SimpleMove_PathfindAndMoveTo(oldGridTransitionPosition, false));
        TaskManager.Enqueue(() => !VNavmesh_IPCSubscriber.Path_IsRunning() && !VNavmesh_IPCSubscriber.Nav_PathfindInProgress());
        TaskManager.Enqueue(GenericHelpers.IsScreenReady);
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Nav_IsReady());
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.SimpleMove_PathfindAndMoveTo(oldGridBowerHousePosition, false));
        TaskManager.Enqueue(() => !VNavmesh_IPCSubscriber.Path_IsRunning() && !VNavmesh_IPCSubscriber.Nav_PathfindInProgress());
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Path_Stop());
        TaskManager.Enqueue(MoveToNearestMarketboard);
    }

    private void MoveToNearestMarketboard()
    {
        var marketBoard = Svc.Objects.Where(o => o.DataId == marketboardDataId)
            .OrderBy(o => Vector3.Distance(o.Position, Player.Position)).FirstOrDefault();
        if (marketBoard == null)
        {
            Svc.Log.Error($"Unable to find marketboard for {Svc.ClientState.TerritoryType}.");
            return;
        }

        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Nav_IsReady());
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.SimpleMove_PathfindAndMoveTo(marketBoard.Position, false));
        TaskManager.Enqueue(() => Vector3.Distance(marketBoard.Position, Player.Position) < 2.9);
        TaskManager.Enqueue(() => VNavmesh_IPCSubscriber.Path_Stop());
        TaskManager.Enqueue(() => InteractWithObject(marketBoard));
    }

    private unsafe void InteractWithObject(IGameObject obj)
    {
        var targetSystem = TargetSystem.Instance();
        if (targetSystem == null)
        {
            Svc.Log.Error($"TargetSystem was null.");
        }
        targetSystem->OpenObjectInteraction(
            (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address);
    }
}