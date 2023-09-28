using Accountant.Classes;
using Accountant.Util;
using Dalamud.Logging;

namespace Accountant.Timers;

public sealed class RetainerTimers : TimersBase<PlayerInfo, RetainerInfo[]>
{
    protected override string FolderName
        => "retainers";

    protected override string SaveError
        => "Could not write retainer data";

    protected override string ParseError
        => "Invalid retainer data file could not be parsed";

    protected override string LoadError
        => "Error loading retainer timers";

    public bool AddOrUpdateRetainer(PlayerInfo player, RetainerInfo retainer, byte slot)
    {
        if (slot >= RetainerInfo.MaxSlots)
        {
            Dalamud.Log.Error($"Only {RetainerInfo.MaxSlots} retainers supported.");
            return false;
        }

        if (retainer.RetainerId == 0)
            return false;

        if (!InternalData.TryGetValue(player, out var retainerList))
        {
            retainerList       = RetainerInfo.GenerateDefaultArray();
            retainerList[slot] = retainer;
            InternalData[player]      = retainerList;
            Invoke();
            return true;
        }

        var oldRetainer = retainerList[slot];
        if (oldRetainer.VentureId == retainer.VentureId
         && Helpers.DateTimeClose(oldRetainer.Venture, retainer.Venture)
         && oldRetainer.RetainerId == retainer.RetainerId
         && oldRetainer.Name == retainer.Name
         && oldRetainer.JobId == retainer.JobId)
            return false;

        retainerList[slot] = retainer;
        Invoke();
        return true;
    }

    public bool ClearRetainer(PlayerInfo player, byte slot)
    {
        if (slot >= RetainerInfo.MaxSlots)
        {
            Dalamud.Log.Error($"Only {RetainerInfo.MaxSlots} retainers supported.");
            return false;
        }

        if (!InternalData.TryGetValue(player, out var retainerList))
            return false;

        var ret = retainerList[slot].RetainerId != 0;
        retainerList[slot] = RetainerInfo.None;

        if (!ret)
            return false;

        Invoke();
        return true;
    }
}
