using System;
using Accountant.Timers;
using Accountant.Util;
using Newtonsoft.Json;

namespace Accountant.Classes;

public struct FreeCompanyInfo : IEquatable<FreeCompanyInfo>, ITimerIdentifier
{
    public string Name     { get; }
    public string Tag      { get; set; }
    public string Leader   { get; set; }
    public ushort ServerId { get; }
    
    [JsonConstructor]
    public FreeCompanyInfo(string name, ushort serverId, string? tag = null, string? leader = null)
    {
        Name     = name;
        ServerId = serverId;
        Tag      = tag ?? string.Empty;
        Leader   = leader ?? string.Empty;
    }

    public bool Equals(FreeCompanyInfo other)
        => ServerId == other.ServerId
         && Name == other.Name;

    public override int GetHashCode()
        => HashCode.Combine(Name, ServerId);

    public override string ToString()
        => $"{Name} ({Accountant.GameData?.GetWorldName(ServerId) ?? ServerId.ToString()}";

    public uint IdentifierHash()
        => (uint)Helpers.CombineHashCodes(Helpers.GetStableHashCode(Name), ServerId);

    public bool Valid()
        => Tag.Length > 0 && Name.Length > 0 && Accountant.GameData.IsValidWorldId(ServerId);

    [JsonIgnore]
    public string CastedName
        => $"{Name}{(char)ServerId}";

    public static PlayerInfo FromCastedName(string castedName)
        => new(castedName[..^1], castedName[^1]);

    public override bool Equals(object? obj)
        => obj is FreeCompanyInfo info && Equals(info);

    public static bool operator ==(FreeCompanyInfo left, FreeCompanyInfo right)
        => left.Equals(right);

    public static bool operator !=(FreeCompanyInfo left, FreeCompanyInfo right)
        => !(left == right);
}
