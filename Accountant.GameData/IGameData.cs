using Accountant.Enums;
using Accountant.Internal;
using Accountant.Structs;
using Dalamud.Data;
using Dalamud.Game;

namespace Accountant;

public static class GameDataFactory
{
    public static IGameData Create(DataManager data)
        => new GameData(data);
}

public interface IGameData
{
    public int Version { get; }

    // Obtain the crop data for the crop or seed with the given ID.
    // If the ID does not correspond to any crop or seed, returns item 0 with times (0, 0).
    public (CropData Data, string Name) FindCrop(uint itemId);

    // Obtain the crop data for the crop or seed with the given name or singular (case insensitive).
    // If the name does not correspond to any crop or seed, returns item 0 with times (0, 0).
    public CropData FindCrop(string name);

    // Obtain the PlotSize for a specific housing plot in a specific housing zone.
    PlotSize GetPlotSize(InternalHousingZone zone, ushort plot);
}
