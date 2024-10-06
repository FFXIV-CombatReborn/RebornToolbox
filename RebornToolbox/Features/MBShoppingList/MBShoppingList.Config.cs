using RebornToolbox.Features.MBShoppingList.Models;

namespace RebornToolbox.Features.MBShoppingList;

public class MBShoppingList_Config
{
    public bool Enabled { get; set; } = false;
    public RegionType ShoppingRegion { get; set; } = RegionType.NorthAmerica;
    public int MaxResults { get; set; } = 100;
    public int LifeStreamTimeout { get; set; } = 300;
    public bool UseVnavPathing { get; set; } = false;
    public bool RemoveQuantityAutomatically { get; set; } = false;
}