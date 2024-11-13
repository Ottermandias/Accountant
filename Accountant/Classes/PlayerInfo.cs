using System;
using Accountant.Timers;
using Accountant.Util;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Newtonsoft.Json;

namespace Accountant.Classes;

[method: JsonConstructor]
public readonly struct PlayerInfo(string name, ushort serverId) : IEquatable<PlayerInfo>, ITimerIdentifier
{
    public string Name     { get; } = name;
    public ushort ServerId { get; } = serverId;

    public PlayerInfo(IPlayerCharacter character)
        : this(character.Name.TextValue, (ushort)character.HomeWorld.RowId)
    { }

    public bool Equals(PlayerInfo other)
        => ServerId == other.ServerId
         && Name == other.Name;

    public override bool Equals(object? obj)
        => obj is PlayerInfo other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Name, ServerId);

    public uint IdentifierHash()
        => (uint)Helpers.CombineHashCodes(Helpers.GetStableHashCode(Name), ServerId);

    public bool Valid()
        => Name.Length > 0 && Name.Contains(' ') && Accountant.GameData.IsValidWorldId(ServerId);

    [JsonIgnore]
    public string CastedName
        => $"{Name}{(char)ServerId}";

    public static PlayerInfo FromCastedName(string castedName)
        => new(castedName[..^1], castedName[^1]);

    public static bool operator ==(PlayerInfo left, PlayerInfo right)
        => left.Equals(right);

    public static bool operator !=(PlayerInfo left, PlayerInfo right)
        => !(left == right);
}
