using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.SeFunctions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace Accountant.Manager;

public class DemolitionManager : IDisposable
{
    public const byte DefaultDisplayMax         = 45;
    public const byte DefaultDisplayWarningFrom = 40;
    public const byte DefaultDisplayFrom        = 30;

    private readonly IClientState        _clientState;
    private readonly IObjectTable        _objects;
    private readonly IFramework          _framework;
    private readonly PositionInfoAddress _position;
    private readonly string              _filePath;
    public           DateTime            LastChangeTime      { get; private set; } = DateTime.UnixEpoch;
    public           bool                FrameworkSubscribed { get; private set; }

    public class DemolitionInfo
    {
        public readonly HashSet<PlayerInfo> CheckedPlayers     = [];
        public          string              Name               = string.Empty;
        public          DateTime            LastVisit          = DateTime.UtcNow.AddDays(-DefaultDisplayMax);
        public          byte                DisplayFrom        = DefaultDisplayFrom;
        public          byte                DisplayWarningFrom = DefaultDisplayWarningFrom;
        public          bool                Tracked            = true;

        public int LastVisitDays
            => (int)(Math.Ceiling((DateTime.UtcNow - LastVisit).TotalDays) + 0.5);
    }

    public readonly Dictionary<PlotInfo, DemolitionInfo> Data = [];

    public PlotInfo CurrentPlot
        => new(_position.Zone, _position.Ward, InsideHouse(_clientState.TerritoryType) ? _position.House : _position.Plot,
            (ushort)(_clientState.LocalPlayer?.CurrentWorld.Id ?? 0));

    public DemolitionManager(AccountantConfiguration config, IDalamudPluginInterface pluginInterface, IClientState clientState,
        IFramework framework, PositionInfoAddress position, IObjectTable objects)
    {
        _clientState = clientState;
        _framework   = framework;
        _position    = position;
        _objects     = objects;
        _filePath    = Path.Combine(pluginInterface.GetPluginConfigDirectory(), "demolitiontracking.json");
        Load(config);
        _clientState.TerritoryChanged += OnTerritoryChange;
        OnTerritoryChange(clientState.TerritoryType);
    }

    public event Action? Change;

    public void Dispose()
    {
        _clientState.TerritoryChanged -= OnTerritoryChange;
        _framework.Update             -= OnFramework;
    }

    public bool CanAddPlot(PlotInfo plotInfo)
        => plotInfo.Valid() && !Data.ContainsKey(plotInfo);

    public bool AddPlot(PlotInfo plotInfo)
    {
        if (!CanAddPlot(plotInfo))
            return false;

        Data.Add(plotInfo, new DemolitionInfo());
        Save();
        return true;
    }

    public DateTime GetWriteTime()
        => !File.Exists(_filePath) ? DateTime.UnixEpoch : File.GetLastWriteTimeUtc(_filePath);

    public void Save()
    {
        try
        {
            var obj = new JObject();
            foreach (var (plot, demo) in Data)
            {
                obj[plot.Value.ToString()] = new JObject
                {
                    [nameof(DemolitionInfo.Name)]               = demo.Name,
                    [nameof(DemolitionInfo.CheckedPlayers)]     = JArray.FromObject(demo.CheckedPlayers.Select(s => s.CastedName)),
                    [nameof(DemolitionInfo.LastVisit)]          = new DateTimeOffset(demo.LastVisit).ToUnixTimeMilliseconds(),
                    [nameof(DemolitionInfo.DisplayFrom)]        = demo.DisplayFrom,
                    [nameof(DemolitionInfo.DisplayWarningFrom)] = demo.DisplayWarningFrom,
                    [nameof(DemolitionInfo.Tracked)]            = demo.Tracked,
                };
            }

            var text = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(_filePath, text);
            LastChangeTime = File.GetLastWriteTimeUtc(_filePath);
            Change?.Invoke();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Could not save demolition manager to {_filePath}:\n{ex}");
        }
    }

    public void Reload()
    {
        Data.Clear();
        Load(null);
        Change?.Invoke();
    }

    internal void Test()
    {
        var plotInfo = new PlotInfo(InternalHousingZone.Mist, 1, 1, (ushort)Accountant.GameData.Worlds().First().Id);
        while (!CanAddPlot(plotInfo) && plotInfo.Valid())
            plotInfo = new PlotInfo(plotInfo.Zone, plotInfo.Ward, (ushort)(plotInfo.Plot + 1), plotInfo.ServerId);
        var demo = new DemolitionInfo()
        {
            DisplayFrom        = 20,
            DisplayWarningFrom = 25,
            Name               = "TEST",
            Tracked            = true,
            LastVisit          = DateTime.UtcNow.AddDays(-25 + 1).AddSeconds(5),
        };
        Data.Add(plotInfo, demo);
        Change?.Invoke();
        Task.Run(async () =>
        {
            await Task.Delay(10000);
            await _framework.RunOnFrameworkThread(() =>
            {
                Data.Remove(plotInfo);
                Change?.Invoke();
            });
        });
    }

    private void Load(AccountantConfiguration? config)
    {
        if (!File.Exists(_filePath))
        {
            LastChangeTime = DateTime.UtcNow;
            return;
        }

        try
        {
            LastChangeTime = File.GetLastWriteTimeUtc(_filePath);
            var text = File.ReadAllText(_filePath);
            var obj  = JObject.Parse(text);
            foreach (var (key, token) in obj)
            {
                if (!ulong.TryParse(key, out var keyValue))
                {
                    Dalamud.Log.Warning($"Invalid key {key} in demolition manager. Skipped.");
                    continue;
                }

                var plotInfo = PlotInfo.FromValue(keyValue);
                if (!plotInfo.Valid())
                {
                    Dalamud.Log.Warning($"Invalid plot info {plotInfo} in demolition manager. Skipped.");
                    continue;
                }

                if (token is not JObject demoObj)
                {
                    Dalamud.Log.Warning($"Invalid data for {plotInfo} in demolition manager. Skipped.");
                    continue;
                }

                var demo = new DemolitionInfo
                {
                    DisplayFrom        = demoObj[nameof(DemolitionInfo.DisplayFrom)]?.ToObject<byte>() ?? DefaultDisplayFrom,
                    DisplayWarningFrom = demoObj[nameof(DemolitionInfo.DisplayWarningFrom)]?.ToObject<byte>() ?? DefaultDisplayWarningFrom,
                    Tracked            = demoObj[nameof(DemolitionInfo.Tracked)]?.ToObject<bool>() ?? true,
                    Name               = demoObj[nameof(DemolitionInfo.Name)]?.ToObject<string>() ?? string.Empty,
                };
                demo.DisplayFrom        = Math.Clamp(demo.DisplayFrom,        (byte)0, DefaultDisplayMax);
                demo.DisplayWarningFrom = Math.Clamp(demo.DisplayWarningFrom, (byte)0, DefaultDisplayMax);

                if (demoObj[nameof(DemolitionInfo.LastVisit)]?.ToObject<long>() is not { } lastVisitTimeStamp)
                {
                    Dalamud.Log.Warning($"No data for {plotInfo}'s last visit for demolition tracking. Set to warning time.");
                    demo.LastVisit = DateTime.UtcNow.AddDays(demo.DisplayWarningFrom);
                }
                else
                {
                    demo.LastVisit = DateTimeOffset.FromUnixTimeMilliseconds(lastVisitTimeStamp).DateTime;
                }

                if (demoObj[nameof(DemolitionInfo.CheckedPlayers)]?.ToObject<string[]>() is { } players)
                    foreach (var player in players)
                        demo.CheckedPlayers.Add(PlayerInfo.FromCastedName(player));

                if (!Data.TryAdd(plotInfo, demo))
                    Dalamud.Log.Warning($"Could not add {plotInfo} to demolition tracking.");
            }
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Unknown error loading demolition manager:\n{ex}");
        }

        if (config == null)
            return;

        var anyAdded = false;
        foreach (var (plotValue, name) in config.PlotNames)
        {
            var plotInfo = PlotInfo.FromValue(plotValue);
            anyAdded |= Data.TryAdd(plotInfo, new DemolitionInfo
            {
                LastVisit = DateTime.UtcNow.AddDays(-DefaultDisplayMax),
                Name      = name,
                Tracked   = false,
            });
        }

        if (anyAdded)
        {
            Save();
            config.Save();
        }
    }

    private void OnTerritoryChange(ushort territory)
    {
        if (InsideHouse(territory))
        {
            _framework.Update   += OnFramework;
            FrameworkSubscribed =  true;
        }
        else
        {
            _framework.Update   -= OnFramework;
            FrameworkSubscribed =  false;
        }
    }


    public static bool InsideHouse(ushort territory)
        => (HousingZone)territory switch
        {
            HousingZone.CottageMist         => true,
            HousingZone.CottageLavenderBeds => true,
            HousingZone.CottageGoblet       => true,
            HousingZone.CottageShirogane    => true,
            HousingZone.CottageEmpyreum     => true,
            HousingZone.HouseMist           => true,
            HousingZone.HouseLavenderBeds   => true,
            HousingZone.HouseGoblet         => true,
            HousingZone.HouseShirogane      => true,
            HousingZone.HouseEmpyreum       => true,
            HousingZone.MansionMist         => true,
            HousingZone.MansionLavenderBeds => true,
            HousingZone.MansionGoblet       => true,
            HousingZone.MansionShirogane    => true,
            HousingZone.MansionEmpyreum     => true,
            _                               => false,
        };


    private void OnFramework(IFramework framework)
    {
        if (_clientState.LocalPlayer is not { } player)
            return;

        var plotInfo = new PlotInfo(_position.Zone, _position.Ward, _position.House, (ushort)player.CurrentWorld.Id);
        if (!Data.TryGetValue(plotInfo, out var demoInfo) || !demoInfo.Tracked)
            return;

        if ((DateTime.UtcNow - demoInfo.LastVisit).TotalMinutes < 10 || demoInfo.CheckedPlayers.Count == 0)
            return;

        for (var i = 0; i < 200; i += 2)
        {
            if (_objects[i] is not IPlayerCharacter pc)
                continue;

            if (!demoInfo.CheckedPlayers.Contains(new PlayerInfo(pc)))
                continue;

            demoInfo.LastVisit = DateTime.UtcNow;
            Save();
            break;
        }
    }
}
