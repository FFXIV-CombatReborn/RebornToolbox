namespace RebornToolbox.Features.ChocoboRacing;

public class ChocoboRacing_Config
{
    public bool Enabled { get; set; } = false;
    public bool AlwaysRun { get; set; } = true;
    public bool AutoRun { get; set; } = true;
    public bool StopAtMaxRank { get; set; } = false;
    public RaceRoute RaceRoute { get; set; } = RaceRoute.Random;
}

public enum RaceRoute : byte
{
    Random = 21,
    Sagolii = 18,
    Costa = 19,
    Tranquil = 20
}

public static class RaceRouteExtensions
{
    public static string ToFriendlyString(this RaceRoute raceRoute)
    {
        switch (raceRoute)
        {
            case RaceRoute.Random:
                return "Random";
            case RaceRoute.Sagolii:
                return "Sagolii Road";
            case RaceRoute.Costa:
                return "Costa del Sol";
            case RaceRoute.Tranquil:
                return "Tranquil Paths";
            default:
                return "Unknown";
        }
    }
}