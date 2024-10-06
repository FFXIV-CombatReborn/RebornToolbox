using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace RebornToolbox.Features.ChocoboRacing;

public class ChocoboRacing_UI : Window
{
    private readonly ChocoboRacing _chocoboRacing;
    public ChocoboRacing_UI(ChocoboRacing chocoboRacing) : base("Chocobo Racing", ImGuiWindowFlags.None, false)
    {
        _chocoboRacing = chocoboRacing;
    }

    public override void Draw()
    {
        
    }
}