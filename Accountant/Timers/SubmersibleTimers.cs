using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Util;
using Dalamud.Logging;

namespace Accountant.Timers;

public sealed class SubmersibleTimers : TimersBase<FreeCompanyInfo, MachineInfo[]>
{
    protected override string FolderName
        => "submersibles";

    protected override string SaveError
        => "Could not write submersible data";

    protected override string ParseError
        => "Invalid submersible data file could not be parsed";

    protected override string LoadError
        => "Error loading submersible timers";

    public bool AddOrUpdateSubmersible(FreeCompanyInfo company, MachineInfo submersible, byte slot)
    {
        if (slot >= MachineInfo.MaxSlots)
        {
            PluginLog.Error($"Only {MachineInfo.MaxSlots} submersibles supported.");
            return false;
        }

        if (submersible.Type != MachineType.Submersible || !submersible.Name.Any())
            return false;

        if (!InternalData.TryGetValue(company, out var submersibles))
        {
            submersibles       = MachineInfo.GenerateDefaultArray();
            submersibles[slot] = submersible;
            InternalData[company]     = submersibles;
            Invoke();
            return true;
        }

        var oldMachine = submersibles[slot];
        if (Helpers.DateTimeClose(oldMachine.Arrival, submersible.Arrival) && oldMachine.Name == submersible.Name)
            return false;

        submersibles[slot] = submersible;
        Invoke();
        return true;
    }

    public bool ClearSubmersible(FreeCompanyInfo company, byte slot)
    {
        if (slot >= MachineInfo.MaxSlots)
        {
            PluginLog.Error($"Only {MachineInfo.MaxSlots} submersibles supported.");
            return false;
        }
        if (!InternalData.TryGetValue(company, out var submersibles))
            return false;
        if (submersibles[slot].Type != MachineType.Submersible)
            return false;
        submersibles[slot] = MachineInfo.None;
        Invoke();
        return true;
    }
}
