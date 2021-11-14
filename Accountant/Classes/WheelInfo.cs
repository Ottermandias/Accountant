using System;
using System.Linq;

namespace Accountant.Classes;

public class WheelInfo
{
    public const int MaxSlots = 6;

    public static int MaxSlotsType(byte grade)
        => 4 - grade;

    public static int HoursType(byte grade)
        => grade switch
        {
            1 => 20,
            2 => 45,
            3 => 70,
            _ => 0,
        };

    public DateTime End()
        => Placed.AddHours(HoursType(Grade));

    public DateTime Placed;
    public uint     ItemId;
    public bool     Accurate;
    public byte     Grade;

    public static readonly WheelInfo None = new() { Accurate = true };

    public static WheelInfo[] GenerateDefaultArray()
        => Enumerable.Repeat(None, MaxSlots).ToArray();
}