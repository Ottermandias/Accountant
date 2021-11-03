using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Data;
using Accountant.Enums;
using Accountant.SeFunctions;
using Accountant.Structs;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Internal;

internal class GameData : IGameData
{
    public const            int                       CurrentVersion = 1;
    private static          Crops?                    _crops;
    private static readonly Plots                     Plots = new();
    private static          Dictionary<uint, string>? _worldNames;
    private static          Dictionary<string, uint>? _worldIds;
    private static          FreeCompanyTracker?       _fcTracker;
    private static          int                       _subscribers = 0;
    public                  bool                      Valid { get; private set; } = true;

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

    public string GetWorldName(uint id)
        => _worldNames!.TryGetValue(id, out var ret)
            ? ret
            : throw new ArgumentOutOfRangeException($"{id} is not a valid world id.");

    public string GetWorldName(PlayerCharacter player)
        => GetWorldName(player.HomeWorld.Id);

    public uint GetWorldId(string worldName)
        => _worldIds!.TryGetValue(worldName, out var ret)
            ? ret
            : throw new ArgumentOutOfRangeException($"{worldName} is not a valid world id.");

    public GameData(GameGui gui, ClientState state, DataManager data)
    {
        _crops     ??= new Crops(data);
        _fcTracker ??= new FreeCompanyTracker(gui, state);
        _worldNames ??= data.GameData.GetExcelSheet<World>()!
            .Where(w => w.IsPublic && !w.Name.RawData.IsEmpty)
            .ToDictionary(w => w.RowId, w => w.Name.RawString);
        _worldIds ??= _worldNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        ++_subscribers;
        Localization.Initialize(data);
    }

    public (SeString Tag, SeString? Name, SeString? Leader) FreeCompanyInfo()
        => Valid ? _fcTracker!.FreeCompanyInfo : throw new InvalidOperationException("Trying to use disposed GameData.");

    public void Dispose()
    {
        if (!Valid)
            return;

        Valid = false;
        --_subscribers;
        if (_subscribers == 0)
            _fcTracker?.Dispose();
    }
}
