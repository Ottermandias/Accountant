using System;
using System.Linq;
using Accountant.Internal;
using FFXIVClientStructs.FFXIV.Client.Game;

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

    public RetainerInfo(RetainerManager.Retainer r)
    {
        Name       = r.NameString;
        Venture    = Helpers.DateFromTimeStamp(r.VentureComplete);
        RetainerId = r.RetainerId;
        VentureId  = r.VentureId;
        Available  = r.Available;
        JobId      = r.ClassJob;
    }

    private RetainerInfo()
    { }

    public static RetainerInfo[] GenerateDefaultArray()
        => Enumerable.Repeat(None, MaxSlots).ToArray();
}
