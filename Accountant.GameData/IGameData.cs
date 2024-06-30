using System;
using System.Collections.Generic;
using Accountant.Enums;
using Accountant.Internal;
using Accountant.Structs;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace Accountant;

public static class GameDataFactory
{
    public static IGameData Create(IPluginLog log, IGameGui gui, IClientState state, IFramework framework, IDataManager data)
        => new GameData(log, gui, state, framework, data);
}

public interface IGameData : IDisposable
{
    public int Version { get; }

    // Obtain the crop data for the crop or seed with the given ID.
    // If the ID does not correspond to any crop or seed, returns item 0 with times (0, 0).
    public (CropData Data, string Name) FindCrop(uint itemId);

    // Obtain the crop data for the crop or seed with the given name or singular (case insensitive).
    // If the name does not correspond to any crop or seed, returns item 0 with times (0, 0).
    public CropData FindCrop(string name);

    // Obtain the corresponding item, full, corrected Name and grade (1-3) to an aetherial wheel id or name.
    // If the given parameter does not correspond to a aetherial wheel, Grade is 0, Name is empty and Item is default constructed.
    public (Item Item, string Name, byte Grade) FindWheel(uint itemId);
    public (Item Item, string Name, byte Grade) FindWheel(string name);

    // Obtain the corresponding item to a treasure hunt map, if it represents one.
    public Item? FindMap(uint itemId);

    // Obtain the number of available wards in a given housing zone.
    public int GetNumWards(InternalHousingZone zone = InternalHousingZone.Mist);

    // Obtain the number of available plots in a given housing zone.
    public int GetNumPlots(InternalHousingZone zone = InternalHousingZone.Mist);

    // Obtain the PlotSize for a specific housing plot in a specific housing zone.
    PlotSize GetPlotSize(InternalHousingZone zone, ushort plot);

    // Obtain an enumerable list of all valid world names and their ids.
    public IEnumerable<(string Name, uint Id)> Worlds();

    // Check whether a world id is a valid id for a player world.
    public bool IsValidWorldId(uint id);

    // Obtain the name of a world for a given world id.
    public string GetWorldName(uint id);

    // Obtain the name of the homeworld for a player character.
    public string GetWorldName(IPlayerCharacter player);

    // Obtain the id of a world by its name.
    public uint GetWorldId(string worldName);

    // Obtain the hour the jumbo cactpot resets on a specific world.
    public byte GetJumboCactpotResetHour(uint worldId);

    // Checks if the GameData is still valid.
    public bool Valid { get; }

    // Returns tag, name and the name of the leader of your current FC.
    // Throws InvalidOperationException if not Valid;
    public (string Tag, string? Name, string? Leader) FreeCompanyInfo();
}
