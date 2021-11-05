using System;
using Accountant.Enums;
using Accountant.Util;

namespace Accountant.Classes;

public readonly struct PlotInfo : IEquatable<PlotInfo>
{
    public readonly InternalHousingZone Zone;
    public readonly ushort              ServerId;
    public readonly ushort              Ward;
    public readonly ushort              Plot;

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

    public ulong Value
        => Plot | ((ulong)Ward << 16) | ((ulong)Zone << 32) | ((ulong)ServerId << 48);

    public string GetName()
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

    public int GetStableHashCode()
        => Helpers.CombineHashCodes((int)Zone, ServerId, Ward, Plot);

    public static PlotInfo FromValue(ulong value)
        => new((InternalHousingZone)((value >> 32) & 0xFFFF)
            , (ushort)((value >> 16) & 0xFFFF)
            , (ushort)(value & 0xFFFF)
            , (ushort)(value >> 48));
}
