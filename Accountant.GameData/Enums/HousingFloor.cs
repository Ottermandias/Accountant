namespace Accountant.Enums;

// The different indoor floors of a house as used by the game.
public enum HousingFloor : byte
{
    Unknown = 0xFF,
    Ground  = 0,
    First   = 1,
    Cellar  = 0x0A,
}
