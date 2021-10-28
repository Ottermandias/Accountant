using System;
using Accountant.Data;
using Accountant.Enums;
using Accountant.Structs;
using Dalamud.Data;

namespace Accountant.Internal;

internal class GameData : IGameData
{
    public const            int    CurrentVersion = 1;
    private static          Crops? _crops;
    private static readonly Plots  Plots = new();

    public int Version
        => CurrentVersion;

    private static InvalidOperationException NotReadyException()
        => new("Accessing Accountant.GameData before Initialize.");

    public (CropData Data, string Name) FindCrop(uint itemId)
        => _crops?.Find(itemId) ?? throw NotReadyException();

    public CropData FindCrop(string name)
        => _crops?.Find(name) ?? throw NotReadyException();

    public PlotSize GetPlotSize(InternalHousingZone zone, ushort plot)
        => Plots.GetSize(zone, plot);

    public GameData(DataManager data)
    {
        _crops ??= new Crops(data);
        Localization.Initialize(data);
    }
}
