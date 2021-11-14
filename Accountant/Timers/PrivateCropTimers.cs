using System;
using System.Numerics;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Structs;

namespace Accountant.Timers;

public sealed class PrivateCropTimers : TimersBase<PlayerInfo, PlantInfo[]>
{
    public const ConfigFlags NameFlags     = ConfigFlags.Crops;
    public const ConfigFlags RequiredFlags = ConfigFlags.Enabled | NameFlags;


    protected override string FolderName
        => "crops_private";

    protected override string SaveError
        => "Could not write private crop data";

    protected override string ParseError
        => "Invalid private crop data file could not be parsed";

    protected override string LoadError
        => "Error loading private crop timers";

    private bool Update(PlantInfo[] plants, CropSpotType type, Vector3 position, uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime)
    {
        var ret = type switch
        {
            CropSpotType.Apartment when plants[0].CloseEnough(position) => plants[0].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Apartment when plants[1].CloseEnough(position) => plants[1].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Apartment => plants[
                    plants[0].PlantId == 0 ? 0 : plants[1].PlantId == 0 ? 1 : plants[0].PlantTime < plants[1].PlantTime ? 0 : 1]
                .Update(itemId, plantTime, tendTime, fertilizeTime, position),
            CropSpotType.Chambers when plants[2].CloseEnough(position) => plants[2].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Chambers when plants[3].CloseEnough(position) => plants[3].Update(itemId, plantTime, tendTime, fertilizeTime),
            CropSpotType.Chambers => plants[
                    plants[2].PlantId == 0 ? 2 : plants[3].PlantId == 0 ? 3 : plants[2].PlantTime < plants[3].PlantTime ? 2 : 3]
                .Update(itemId, plantTime, tendTime, fertilizeTime, position),
            _ => false,
        };
        if (!ret)
            return false;

        Invoke();
        return true;
    }

    private (PlayerInfo, PlantInfo[]) FindPrivateCrops(CropSpotIdentification id)
    {
        var info = new PlayerInfo(id.PlayerName, id.ServerId);
        if (InternalData.TryGetValue(info, out var beds))
            return (info, beds);

        var ret = new PlantInfo[PlantInfo.PotsPerApartment + PlantInfo.PotsPerChamber];
        InternalData[info] = ret;
        return (info, ret);
    }

    private void UpdateAndSave(CropSpotIdentification id, uint itemId, DateTime? plantTime, DateTime? tendTime, DateTime? fertilizeTime)
    {
        if (id.Type is not (CropSpotType.Apartment or CropSpotType.Chambers))
            return;

        var (info, data) = FindPrivateCrops(id);
        if (Update(data, id.Type, id.Position, itemId, plantTime, tendTime, fertilizeTime))
            Save(info, data);
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
