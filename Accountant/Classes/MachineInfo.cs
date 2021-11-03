using System;
using Accountant.Enums;

namespace Accountant.Classes;

public class MachineInfo
{
    public const int MaxSlots = 4;
    public const int Types    = 2;

    public string      Name;
    public DateTime    Arrival;
    public MachineType Type;

    public MachineInfo(string name, DateTime arrival, MachineType type)
    {
        Name    = name;
        Arrival = arrival;
        Type    = type;
    }

    public int Slot(int slot)
        => Type switch
        {
            MachineType.Airship   => slot < MaxSlots ? slot : -1,
            MachineType.Submersible => slot < MaxSlots ? slot + MaxSlots : -1,
            _                     => -1,
        };

    public static readonly MachineInfo None = new(string.Empty, DateTime.MinValue, MachineType.Unknown);
}
