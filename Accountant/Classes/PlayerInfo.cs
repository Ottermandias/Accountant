using System;
using Accountant.Util;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Newtonsoft.Json;

namespace Accountant.Classes;

public readonly struct PlayerInfo : IEquatable<PlayerInfo>
{
    public readonly string Name;
    public readonly uint   ServerId;

    [JsonConstructor]
    public PlayerInfo(string name, uint serverId)
    {
        Name     = name;
        ServerId = serverId;
    }

    public PlayerInfo(PlayerCharacter character)
        : this(character.Name.TextValue, character.HomeWorld.Id)
    { }

    public bool Equals(PlayerInfo other)
        => ServerId == other.ServerId
         && Name == other.Name;

    public override bool Equals(object? obj)
        => obj is PlayerInfo other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Name, ServerId);

    public int GetStableHashCode()
        => Helpers.CombineHashCodes(Helpers.GetStableHashCode(Name), (int) ServerId);
}
