using System;
using Accountant.Classes;
using Dalamud.Logging;

namespace Accountant.Timers;

public sealed class TaskTimers : TimersBase<PlayerInfo, TaskInfo>
{
    public const ConfigFlags TaskFlags = ConfigFlags.LeveAllowances
      | ConfigFlags.MapAllowance
      | ConfigFlags.Squadron
      | ConfigFlags.MiniCactpot
      | ConfigFlags.JumboCactpot;

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

    public bool AddOrUpdateMiniCactpot(PlayerInfo player, MiniCactpot mini)
    {
        if (!InternalData.TryGetValue(player, out var oldInfo))
        {
            InternalData[player] = new TaskInfo()
            {
                MiniCactpot = mini,
            };
            Invoke();
            return true;
        }

        if (oldInfo.MiniCactpot.Tickets == mini.Tickets && oldInfo.MiniCactpot.NextReset() > mini.LastUpdate)
            return false;

        InternalData[player].MiniCactpot = mini;
        Invoke();
        return true;
    }

    public void CheckJumboCactpotReset(DateTime now)
    {
        foreach (var (player, task) in Data)
        {
            var doubleReset = task.JumboCactpot.NextReset(player.ServerId).AddDays(7);
            if (doubleReset > now)
                continue;

            task.JumboCactpot = new JumboCactpot { LastUpdate = now };
            Save(player, task);
        }
    }

    public bool AddOrUpdateJumboCactpot(PlayerInfo player, JumboCactpot jumbo)
    {
        if (!InternalData.TryGetValue(player, out var oldInfo))
        {
            InternalData[player] = new TaskInfo()
            {
                JumboCactpot = jumbo,
            };
            Invoke();
            return true;
        }

        if (oldInfo.JumboCactpot.EqualTickets(jumbo) && oldInfo.JumboCactpot.NextReset(player.ServerId).AddDays(7) > jumbo.LastUpdate)
            return false;

        InternalData[player].JumboCactpot = jumbo;
        Invoke();
        return true;
    }


    public bool AddOrUpdateMini(PlayerInfo player)
    {
        if (!InternalData.TryGetValue(player, out var oldInfo))
        {
            InternalData[player] = new TaskInfo()
            {
                MiniCactpot = new MiniCactpot()
                {
                    Tickets    = 1,
                    LastUpdate = DateTime.UtcNow,
                },
            };
            Invoke();
            return true;
        }

        var now = DateTime.UtcNow;
        if (now >= oldInfo.MiniCactpot.NextReset())
            oldInfo.MiniCactpot.Tickets = 0;
        if (oldInfo.MiniCactpot.Tickets == MiniCactpot.MaxTickets)
        {
            PluginLog.Error($"Increasing Mini Cactpot Tickets to more than {MiniCactpot.MaxTickets} is not possible.");
            return false;
        }

        ++oldInfo.MiniCactpot.Tickets;
        oldInfo.MiniCactpot.LastUpdate = now;
        Invoke();
        return true;
    }

    public bool AddOrUpdateJumbo(PlayerInfo player, ushort ticket)
    {
        if (!InternalData.TryGetValue(player, out var oldInfo))
        {
            var newInfo = new TaskInfo()
            {
                JumboCactpot = new JumboCactpot() { LastUpdate = DateTime.UtcNow },
            };
            newInfo.JumboCactpot.SetFirstTicket(ticket);
            InternalData[player] = newInfo;
            Invoke();
            return true;
        }

        var now = DateTime.UtcNow;
        if (now >= oldInfo.JumboCactpot.NextReset(player.ServerId))
            oldInfo.JumboCactpot.ClearTickets();
        if (oldInfo.JumboCactpot.IsFull())
        {
            PluginLog.Error($"Buying more than {JumboCactpot.MaxTickets} jumbo cactpot tickets per week is not possible.");
            return false;
        }

        oldInfo.JumboCactpot.SetFirstTicket(ticket);
        oldInfo.JumboCactpot.LastUpdate = now;
        Invoke();
        return true;
    }

    public bool ClearFirstJumbo(PlayerInfo player)
    {
        if (!InternalData.TryGetValue(player, out var oldInfo))
        {
            var newInfo = new TaskInfo()
            {
                JumboCactpot = new JumboCactpot() { LastUpdate = DateTime.UtcNow },
            };
            InternalData[player] = newInfo;
            Invoke();
            return true;
        }

        var now = DateTime.UtcNow;
        if (now >= oldInfo.JumboCactpot.NextReset(player.ServerId))
            oldInfo.JumboCactpot.ClearTickets();
        oldInfo.JumboCactpot.ClearFirstTicket();
        oldInfo.JumboCactpot.LastUpdate = now;
        Invoke();
        return true;
    }
}
