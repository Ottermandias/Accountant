using System;
using System.Numerics;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Structs;

namespace Accountant.Timers;

public sealed class PlotCropTimers : TimersBase<PlotInfo, PlantInfo[]>
{
    protected override string FolderName
        => "crops_plot";

    protected override string SaveError
        => "Could not write plot crop data";

    protected override string ParseError
        => "Invalid plot crop data file could not be parsed";

    protected override string LoadError
        => "Error loading plot crop timers";

    private bool Update(PlantInfo[] plants, Vector3 position, uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime)
    {
        var outdoorPlants  = (ushort)(plants.Length & ~0b111);
        var oldestPlantIdx = outdoorPlants;
        for (var i = (ushort)(plants.Length - 1); i >= outdoorPlants; --i)
        {
            var plant = plants[i];
            if (plant.CloseEnough(position))
            {
                if (!plants[i].Update(itemId, plantTime, tendTime, fertilizeTime))
                    return false;

                Invoke();
                return true;
            }

            if (plant.PlantId == 0 || plant.PlantTime < plants[oldestPlantIdx].PlantTime)
                oldestPlantIdx = i;
        }

        if (!plants[oldestPlantIdx].Update(itemId, plantTime, tendTime, fertilizeTime, position))
            return false;

        Invoke();
        return true;
    }

    private bool Update(PlantInfo[] plants, ushort patch, ushort bed, uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime)
    {
        var idx = (patch << 3) + bed;
        if (idx > plants.Length || !plants[idx].Update(itemId, plantTime, tendTime, fertilizeTime))
            return false;

        Invoke();
        return true;
    }

    private (PlotInfo, PlantInfo[]) FindPlotCrops(CropSpotIdentification id)
    {
        var info = new PlotInfo(id.Zone, id.Ward, id.Plot, id.ServerId);
        if (InternalData.TryGetValue(info, out var beds))
            return (info, beds);

        var size = Accountant.GameData.GetPlotSize(info.Zone, info.Plot).TotalBeds();
        var ret  = new PlantInfo[size];
        InternalData[info] = ret;
        return (info, ret);
    }

    private void UpdateAndSave(CropSpotIdentification id, uint itemId, DateTime? plantTime, DateTime? tendTime, DateTime? fertilizeTime)
    {
        if (id.Type == CropSpotType.House)
        {
            var (info, data) = FindPlotCrops(id);
            if (Update(data, id.Position, itemId, plantTime, tendTime, fertilizeTime))
                Save(info, data);
        }
        else if (id.Type == CropSpotType.Outdoors)
        {
            var (info, data) = FindPlotCrops(id);
            if (Update(data, id.Patch, id.Bed, itemId, plantTime, tendTime, fertilizeTime))
                Save(info, data);
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
}
