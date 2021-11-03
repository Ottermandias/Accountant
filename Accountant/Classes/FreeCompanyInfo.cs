using System;
using Accountant.Util;

namespace Accountant.Classes;

public class FreeCompanyInfo : IEquatable<FreeCompanyInfo>
{
    public readonly string Name;
    public          string Tag;
    public          string Leader;
    public readonly uint   ServerId;

    public FreeCompanyInfo(string name, uint serverId, string? tag = null, string? leader = null)
    {
        Name     = name;
        ServerId = serverId;
        Tag      = tag ?? string.Empty;
        Leader   = leader ?? string.Empty;
    }

    public bool Equals(FreeCompanyInfo? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Name == other.Name
         && ServerId == other.ServerId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((FreeCompanyInfo)obj);
    }

    public override int GetHashCode()
        => HashCode.Combine(Name, ServerId);

    public override string ToString()
        => $"{Name} ({Accountant.GameData?.GetWorldName(ServerId) ?? ServerId.ToString()}";

    public int GetStableHashCode()
        => Helpers.CombineHashCodes(Helpers.GetStableHashCode(Name), (int)ServerId);
}
