using Dalamud;
using Dalamud.Plugin;
using ECommons;

namespace RebornToolbox;

public class Plugin : IDalamudPlugin
{
    public Plugin(DalamudPluginInterface plugin)
    {
        ECommonsMain.Init(plugin, this);
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
    }
}