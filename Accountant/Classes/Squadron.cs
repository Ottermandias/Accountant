using System;
using Lumina.Excel.Sheets;

namespace Accountant.Classes;

public struct Squadron
{
    public DateTime MissionEnd;
    public DateTime TrainingEnd;
    public ushort   MissionId;
    public ushort   TrainingId;
    public bool     NewRecruits;

    public static readonly Squadron None = new()
    {
        MissionEnd  = DateTime.MinValue,
        TrainingEnd = DateTime.MinValue,
    };

    public string? MissionName()
    {
        if (MissionId == 0)
            return null;

        return Dalamud.GameData.GetExcelSheet<GcArmyExpedition>()?
            .GetRowOrDefault(MissionId)?
            .Name.ToString();
    }

    public string? TrainingName()
    {
        if (TrainingId == 0)
            return null;

        return Dalamud.GameData.GetExcelSheet<GcArmyTraining>()?
            .GetRowOrDefault(TrainingId)?
            .Name.ToString();
    }
}
