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

    public override string ToString()
        => $"{Ward:D2}-{Plot:D2}, {Zone.ToName()}";

    public string GetName()
        => Accountant.Config.PlotNames.TryGetValue(this, out var ret) ? ret : ToString();

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
        => Helpers.CombineHashCodes((int) Zone, ServerId, Ward, Plot);
}
