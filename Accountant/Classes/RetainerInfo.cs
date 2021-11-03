using System;
using Accountant.Structs;

namespace Accountant.Classes;

public class RetainerInfo
{
    public const int MaxSlots = 10;

    public string   Name = string.Empty;
    public DateTime Venture;
    public ulong    RetainerId;
    public uint     VentureId;
    public bool     Available;
    public byte     JobId;

    public static readonly RetainerInfo None = new();

    public RetainerInfo(SeRetainer r)
    {
        Name       = r.Name.TextValue;
        Venture    = r.VentureComplete;
        RetainerId = r.RetainerID;
        VentureId  = r.VentureID;
        Available  = r.Available;
        JobId      = r.ClassJob;
    }

    private RetainerInfo()
    { }
}
