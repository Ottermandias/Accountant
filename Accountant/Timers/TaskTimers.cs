using System;
using System.ComponentModel.DataAnnotations;
using Accountant.Classes;

namespace Accountant.Timers;

public sealed class TaskTimers : TimersBase<PlayerInfo, TaskInfo>
{
    public const ConfigFlags TaskFlags = ConfigFlags.LeveAllowances;

    protected override string FolderName
        => "tasks";

    protected override string SaveError
        => "Could not write task data";

    protected override string ParseError
        => "Invalid task data file could not be parsed";

    protected override string LoadError
        => "Error loading task timers";

    public bool AddOrUpdateMap(PlayerInfo player, DateTime value)
    {
        // Add a few seconds so that the timer should not be early,
        // the transmitted timestamps are not 100% accurate for some reason.
        value = value < DateTime.UtcNow.AddSeconds(30) ? DateTime.MinValue : value.AddSeconds(30);
        if (!InternalData.TryGetValue(player, out var tasks))
        {
            InternalData[player] = new TaskInfo
            {
                Map = value,
            };
            Invoke();
            return true;
        }

        if (Math.Abs((tasks.Map - value).TotalMinutes) < 5)
            return false;

        tasks.Map = value;
        Invoke();
        return true;
    }

    public bool AddOrUpdateLeves(PlayerInfo player, int leves)
    {
        leves = leves < 0 ? 0 : leves > Leve.AllowanceCap ? Leve.AllowanceCap : leves;
        var updateTime = Leve.Round(DateTime.UtcNow);
        var newLeve = new Leve
        {
            Allowances = leves,
            LastUpdate = updateTime,
        };
        if (!InternalData.TryGetValue(player, out var tasks))
        {
            InternalData[player] = new TaskInfo
            {
                Leves = newLeve,
            };
            Invoke();
            return true;
        }

        var oldLeve = tasks.Leves;
        if (oldLeve.Allowances == leves && oldLeve.LastUpdate == updateTime)
            return false;

        tasks.Leves = newLeve;
        Invoke();
        return true;
    }

    public bool AddOrUpdateSquadron(PlayerInfo player, Squadron squadron)
    {
        if (!InternalData.TryGetValue(player, out var tasks))
        {
            InternalData[player] = new TaskInfo
            {
                Squadron = squadron,
            };
            Invoke();
            return true;
        }

        if (tasks.Squadron.MissionEnd == squadron.MissionEnd
         && tasks.Squadron.TrainingEnd == squadron.TrainingEnd
         && tasks.Squadron.TrainingId == squadron.TrainingId
         && tasks.Squadron.MissionId == squadron.MissionId
         && tasks.Squadron.NewRecruits == squadron.NewRecruits)
            return false;

        tasks.Squadron = squadron;
        Invoke();
        return true;
    }
}
