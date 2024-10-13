using Dalamud.Game.ClientState.Keys;

namespace RebornToolbox.Features.ChocoboRacing;

public class ChocoboRacing_Config
{
    public bool Enabled { get; set; } = false;
    public bool AlwaysRun { get; set; } = true;
    public bool AutoRun { get; set; } = true;
    public bool StopAtMaxRank { get; set; } = false;
    public RaceRoute RaceRoute { get; set; } = RaceRoute.Random;
    public VirtualKey MoveForwardKey { get; set; } = VirtualKey.W;
}

public enum RaceRoute : byte
{
    Random = 21,
    Sagolii = 18,
    Costa = 19,
    Tranquil = 20,
    Random_No_Rewards = 25,
    Tranquil_No_Rewards = 24,
    Costa_No_Rewards = 23,
    Sagolii_No_Rewards = 22,
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
            case RaceRoute.Random_No_Rewards:
                return "Random (No Rewards)";
            case RaceRoute.Tranquil_No_Rewards:
                return "Tranquil Paths (No Rewards)";
            case RaceRoute.Sagolii_No_Rewards:
                return "Sagolii Road (No Rewards)";
            case RaceRoute.Costa_No_Rewards:
                return "Costa del Sol (No Rewards)";
            default:
                return "Unknown";
        }
    }
}