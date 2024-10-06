using Dalamud.Game.Inventory.InventoryEventArgTypes;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace RebornToolbox.Features.InvSort;

public class InvSort
{
    private Chat chat;

    public InvSort()
    {
        chat = new Chat();
        Svc.GameInventory.InventoryChanged += OnInventoryChanged;
    }

    private DateTime lastSort = DateTime.MinValue;
    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        if (!Plugin.Configuration.InvSortConfig.Enabled || (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl)))
            return;

        if (DateTime.Now > lastSort.AddSeconds(Plugin.Configuration.InvSortConfig.MinUpdateInterval))
        {
            Svc.Log.Verbose($"Sorting inventory");
            lastSort = DateTime.Now;
            chat.ExecuteCommand("/isort condition inventory ilv des");
            chat.ExecuteCommand("/isort execute inventory");
            chat.ExecuteCommand("/isort clear inventory");
        }
    }
}