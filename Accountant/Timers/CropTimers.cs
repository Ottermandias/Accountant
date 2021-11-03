using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Structs;
using Dalamud.Logging;
using Newtonsoft.Json;
using OtterLoc.Structs;

namespace Accountant.Timers;

public class CropTimers
{
    public const int PotsPerApartment = 2;
    public const int PotsPerChamber   = 2;

    public const     string                              FolderName        = "crops";
    public const     string                              PrivateCropPrefix = "private_";
    public const     string                              PlotCropPrefix    = "plot_";
    private readonly Dictionary<PlayerInfo, PlantInfo[]> _privateCrops     = new();
    private readonly Dictionary<PlotInfo, PlantInfo[]>   _plotCrops        = new();

    public event TimerChange? CropChanged;

    public IReadOnlyDictionary<PlayerInfo, PlantInfo[]> PrivateCrops
        => _privateCrops;

    public IReadOnlyDictionary<PlotInfo, PlantInfo[]> PlotCrops
        => _plotCrops;

    public string GetPrivateName(ushort idx)
        => idx < PotsPerApartment
            ? $"{StringId.Apartment.Value()}, {StringId.CropPot.Value()} {idx + 1}"
            : $"{StringId.Chambers.Value()}, {StringId.CropPot.Value()} {idx + 1 - PotsPerApartment}";

    public string GetPlotName(PlotSize size, ushort idx)
    {
        var s = size.OutdoorBeds();
        return idx < s
            ? $"{StringId.CropBed.Value()} {(idx >> 3) + 1}-{(idx & 0b111) + 1}"
            : $"{StringId.CropPot.Value()} {idx + 1 - s}";
    }

    private bool Update(PlantInfo[] plants, Vector3 position, uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime)
    {
        var outdoorPlants  = (ushort)(plants.Length & ~0b111);
        var oldestPlantIdx = outdoorPlants;
        for (var i = (ushort)(plants.Length - 1); i >= outdoorPlants; --i)
        {
            var plant = plants[i];
            if (plant.CloseEnough(position))
                return plants[i].Update(itemId, plantTime, tendTime, fertilizeTime);

            if (plant.PlantId == 0 || plant.PlantTime < plants[oldestPlantIdx].PlantTime)
                oldestPlantIdx = i;
        }

        if (!plants[oldestPlantIdx].Update(itemId, plantTime, tendTime, fertilizeTime, position))
            return false;

        CropChanged?.Invoke();
        return true;
    }

    private bool Update(PlantInfo[] plants, ushort patch, ushort bed, uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime)
    {
        var idx = (patch << 3) + bed;
        if (idx > plants.Length || !plants[idx].Update(itemId, plantTime, tendTime, fertilizeTime))
            return false;

        CropChanged?.Invoke();
        return true;
    }

    private bool Update(PlantInfo[] plants, CropSpotType type, Vector3 position, uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime)
    {
        var ret = type switch
        {
            CropSpotType.Apartment when plants[0].CloseEnough(position) => plants[0].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Apartment when plants[1].CloseEnough(position) => plants[1].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Apartment => plants[plants[0].PlantId == 0 ? 0 : plants[1].PlantId == 0 ? 1 : plants[0].PlantTime < plants[1].PlantTime ? 0 : 1]
                .Update(itemId, plantTime, tendTime, fertilizeTime, position),
            CropSpotType.Chambers when plants[2].CloseEnough(position) => plants[2].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Chambers when plants[3].CloseEnough(position) => plants[3].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Chambers => plants[plants[2].PlantId == 0 ? 2 : plants[3].PlantId == 0 ? 3 : plants[2].PlantTime < plants[3].PlantTime ? 2 : 3]
                .Update(itemId, plantTime, tendTime, fertilizeTime, position),
            _ => false,
        };
        if (!ret)
            return false;

        CropChanged?.Invoke();
        return true;
    }

    private (PlayerInfo, PlantInfo[]) FindPrivateCrops(CropSpotIdentification id)
    {
        var info = new PlayerInfo(id.PlayerName, id.ServerId);
        if (_privateCrops.TryGetValue(info, out var beds))
            return (info, beds);

        var ret = new PlantInfo[PotsPerApartment + PotsPerChamber];
        _privateCrops[info] = ret;
        return (info, ret);
    }

    private (PlotInfo, PlantInfo[]) FindPlotCrops(CropSpotIdentification id)
    {
        var info = new PlotInfo(id.Zone, id.Ward, id.Plot, id.ServerId);
        if (_plotCrops.TryGetValue(info, out var beds))
            return (info, beds);

        var size = Accountant.GameData.GetPlotSize(info.Zone, info.Plot).TotalBeds();
        var ret  = new PlantInfo[size];
        _plotCrops[info] = ret;
        return (info, ret);
    }

    private bool Update(CropSpotIdentification id, uint itemId, DateTime? plantTime, DateTime? tendTime, DateTime? fertilizeTime)
    {
        return id.Type switch
        {
            CropSpotType.Apartment => Update(FindPrivateCrops(id).Item2, id.Type,     id.Position, itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Chambers  => Update(FindPrivateCrops(id).Item2, id.Type,     id.Position, itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.House     => Update(FindPlotCrops(id).Item2,    id.Position, itemId,      plantTime, tendTime, fertilizeTime),
            CropSpotType.Outdoors  => Update(FindPlotCrops(id).Item2,    id.Patch,    id.Bed,      itemId, plantTime, tendTime, fertilizeTime),
            _                      => false,
        };
    }

    private void UpdateAndSave(CropSpotIdentification id, uint itemId, DateTime? plantTime, DateTime? tendTime, DateTime? fertilizeTime)
    {
        switch (id.Type)
        {
            case CropSpotType.Apartment:
            case CropSpotType.Chambers:
            {
                var (info, data) = FindPrivateCrops(id);
                if (Update(data, id.Type, id.Position, itemId, plantTime, tendTime, fertilizeTime))
                    Save(info, data);

                return;
            }
            case CropSpotType.House:
            {
                var (info, data) = FindPlotCrops(id);
                if (Update(data, id.Position, itemId, plantTime, tendTime, fertilizeTime))
                    Save(info, data);
                return;
            }
            case CropSpotType.Outdoors:
            {
                var (info, data) = FindPlotCrops(id);
                if (Update(data, id.Patch, id.Bed, itemId, plantTime, tendTime, fertilizeTime))
                    Save(info, data);
                return;
            }
        }
    }

    public void HarvestCrop(CropSpotIdentification id)
        => UpdateAndSave(id, 0, null, null, null);

    public void PlantCrop(CropSpotIdentification id, uint itemId, DateTime plantTime)
        => UpdateAndSave(id, itemId, plantTime, null, null);

    public void TendCrop(CropSpotIdentification id, uint itemId, DateTime tendTime)
        => UpdateAndSave(id, itemId, null, tendTime, null);

    public void FertilizeCrop(CropSpotIdentification id, uint itemId, DateTime fertilizeTime)
        => UpdateAndSave(id, itemId, null, null, fertilizeTime);

    public bool RemovePlot(PlotInfo info)
        => _plotCrops.Remove(info);

    public bool RemovePrivate(PlayerInfo info)
        => _privateCrops.Remove(info);

    private static DirectoryInfo CreateFolder()
    {
        var path = Path.Combine(Dalamud.PluginInterface.ConfigDirectory.FullName, FolderName);
        return Directory.CreateDirectory(path);
    }

    public void Save(PlayerInfo player, PlantInfo[] plants)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{PrivateCropPrefix}{player.GetStableHashCode():X8}.json");
            var data     = JsonConvert.SerializeObject((player, plants), Formatting.Indented);
            File.WriteAllText(fileName, data);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write private crop data:\n{e}");
        }
    }

    public void Save(PlotInfo plot, PlantInfo[] plants)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{PlotCropPrefix}{plot.GetStableHashCode():X8}.json");
            var data     = JsonConvert.SerializeObject((plot, plants), Formatting.Indented);
            File.WriteAllText(fileName, data);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write plot crop data:\n{e}");
        }
    }

    public void Save(PlayerInfo player)
    {
        if (_privateCrops.TryGetValue(player, out var crops))
            Save(player, crops);
    }

    public void Save(PlotInfo plot)
    {
        if (_plotCrops.TryGetValue(plot, out var crops))
            Save(plot, crops);
    }

    public void DeleteFile(PlayerInfo player)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"private_{player.GetStableHashCode():X8}.json");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not delete file:\n{e}");
        }
    }

    public void DeleteFile(PlotInfo plot)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"plot_{plot.GetStableHashCode():X8}.json");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not delete file:\n{e}");
        }
    }

    public static CropTimers Load()
    {
        var timers = new CropTimers();
        try
        {
            var folder = CreateFolder();
            foreach (var file in folder.EnumerateFiles("*.json"))
            {
                try
                {
                    var data = File.ReadAllText(file.FullName);
                    if (file.Name.StartsWith("private_"))
                    {
                        var (player, crops)          = JsonConvert.DeserializeObject<(PlayerInfo, PlantInfo[])>(data);
                        timers._privateCrops[player] = crops;
                    }
                    else if (file.Name.StartsWith("plot_"))
                    {
                        var (plot, crops)       = JsonConvert.DeserializeObject<(PlotInfo, PlantInfo[])>(data);
                        timers._plotCrops[plot] = crops;
                    }
                    else
                    {
                        throw new Exception("Invalid or missing prefix.");
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Error($"Invalid crop data file {file} could not be parsed:\n{e}");
                }
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading machine timers:\n{e}");
        }

        return timers;
    }
}
