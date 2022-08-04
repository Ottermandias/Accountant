using System;
using Accountant.Enums;
using Accountant.Timers;
using Accountant.Util;
using Newtonsoft.Json;

namespace Accountant.Classes;

public readonly struct PlotInfo : IEquatable<PlotInfo>, ITimerIdentifier
{
    public InternalHousingZone Zone     { get; }
    public ushort              ServerId { get; }
    public ushort              Ward     { get; }
    public ushort              Plot     { get; }

    public PlotInfo(InternalHousingZone zone, ushort ward, ushort plot, ushort serverId)
    {
        Zone     = zone;
        ServerId = serverId;
        Ward     = ward;
        Plot     = plot;
    }

    public string ToName()
        => $"{Ward:D2}-{Plot:D2}, {Zone.ToName()}";

    public override string ToString()
        => $"{Plot:D2}{Ward:D2}{(ushort)Zone:X4}{ServerId:X4}";

    [JsonIgnore]
    public ulong Value
        => Plot | ((ulong)Ward << 16) | ((ulong)Zone << 32) | ((ulong)ServerId << 48);

    [JsonIgnore]
    public string Name
        => Accountant.Config.PlotNames.TryGetValue(Value, out var ret) ? ret : ToName();

    public bool Equals(PlotInfo other)
        => Zone == other.Zone
         && ServerId == other.ServerId
         && Ward == other.Ward
         && Plot == other.Plot;

    public override bool Equals(object? obj)
        => obj is PlotInfo other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine((int)Zone, ServerId, Ward, Plot);

    public uint IdentifierHash()
        => (uint)Helpers.CombineHashCodes((int)Zone, ServerId, Ward, Plot);

    public bool Valid()
        => Accountant.GameData.IsValidWorldId(ServerId)
         && Enum.IsDefined(Zone)
         && Plot > 0
         && Plot <= Accountant.GameData.GetNumPlots()
         && Ward > 0
         && Ward <= Accountant.GameData.GetNumWards();

    public static PlotInfo FromValue(ulong value)
        => new((InternalHousingZone)((value >> 32) & 0xFFFF)
            , (ushort)((value >> 16) & 0xFFFF)
            , (ushort)(value & 0xFFFF)
            , (ushort)(value >> 48));

    public static bool operator ==(PlotInfo left, PlotInfo right)
        => left.Equals(right);

    public static bool operator !=(PlotInfo left, PlotInfo right)
        => !(left == right);
}
