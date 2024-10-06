using Dalamud.Configuration;
using ECommons.DalamudServices;
using Newtonsoft.Json;
using RebornToolbox.Features.InvSort;
using RebornToolbox.Features.MBShoppingList;

namespace RebornToolbox.Common;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool ExpertMode { get; set; } = false;
    public MBShoppingList_Config ShoppingListConfig { get; set; } = new MBShoppingList_Config();
    public InvSort_Config InvSortConfig { get; set; } = new InvSort_Config();

    public static string ConfigPath => Svc.PluginInterface.ConfigFile.FullName;

    public static Configuration LoadConfig()
    {
        if (!File.Exists(ConfigPath))
            return new Configuration();

        var json = File.ReadAllText(ConfigPath);
        var config = JsonConvert.DeserializeObject<Configuration>(json);
        if (config == null)
        {
            Svc.Log.Error($"Can't load configuration file: {ConfigPath}");
            return new Configuration();
        }

        return config;
    }

    public void SaveConfig()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(ConfigPath, json);
    }
}