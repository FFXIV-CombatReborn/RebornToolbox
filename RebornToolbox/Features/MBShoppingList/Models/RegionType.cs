namespace RebornToolbox.Features.MBShoppingList.Models;

public enum RegionType
{
    NorthAmerica = 0,
    Europe = 1,
    Japan = 2,
    Oceania = 3,
}

public static class RegionTypeExtensions
{
    public static string ToUniversalisString(this RegionType regionType)
    {
        switch (regionType)
        {
            case RegionType.NorthAmerica:
                return "north-america";
            case RegionType.Europe:
                return "europe";
            case RegionType.Japan:
                return "japan";
            case RegionType.Oceania:
                return "oceania";
            default:
                return "north-america";
        }
    }

    public static RegionType ToRegionType(this string regionType)
    {
        switch (regionType)
        {
            case "north-america":
                return RegionType.NorthAmerica;
            case "europe":
                return RegionType.Europe;
            case "japan":
                return RegionType.Japan;
            case "oceania":
                return RegionType.Oceania;
            default:
                return RegionType.NorthAmerica;
        }
    }
}