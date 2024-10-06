using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RebornToolbox.Features.ChocoboRacing;

public class ChocoboRacing_UI : Window
{
    private readonly ChocoboRacing _chocoboRacing;
    public ChocoboRacing_UI(ChocoboRacing chocoboRacing) : base("Feather to the Metal", ImGuiWindowFlags.None, false)
    {
        _chocoboRacing = chocoboRacing;
    }

    public override void Draw()
    {
        var enabled = _chocoboRacing.IsRunning;
        if (ImGui.Checkbox("Enable", ref enabled))
        {
            _chocoboRacing.IsRunning = enabled;
        }
        ImGui.Text($"Current Rank: {_chocoboRacing.ChocoboLevel}");
        ImGui.Text($"Moving: {_chocoboRacing.IsMoving}");
    }
}