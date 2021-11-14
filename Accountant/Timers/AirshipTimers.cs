using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Util;
using Dalamud.Logging;

namespace Accountant.Timers;

public sealed class AirshipTimers : TimersBase<FreeCompanyInfo, MachineInfo[]>
{
    protected override string FolderName
        => "airships";

    protected override string SaveError
        => "Could not write airship data";

    protected override string ParseError
        => "Invalid airship data file could not be parsed";

    protected override string LoadError
        => "Error loading airship timers";

    public bool AddOrUpdateAirship(FreeCompanyInfo company, MachineInfo airship, byte slot)
    {
        if (slot >= MachineInfo.MaxSlots)
        {
            PluginLog.Error($"Only {MachineInfo.MaxSlots} airships supported.");
            return false;
        }

        if (airship.Type != MachineType.Airship || !airship.Name.Any())
            return false;

        if (!InternalData.TryGetValue(company, out var airships))
        {
            airships              = MachineInfo.GenerateDefaultArray();
            airships[slot]        = airship;
            InternalData[company] = airships;
            Invoke();
            return true;
        }

        var oldMachine = airships[slot];
        if (Helpers.DateTimeClose(oldMachine.Arrival, airship.Arrival) && oldMachine.Name == airship.Name)
            return false;

        airships[slot] = airship;
        Invoke();
        return true;
    }

    public bool ClearAirship(FreeCompanyInfo company, byte slot)
    {
        if (slot >= MachineInfo.MaxSlots)
        {
            PluginLog.Error($"Only {MachineInfo.MaxSlots} airships supported.");
            return false;
        }

        if (!InternalData.TryGetValue(company, out var airships))
            return false;
        if (airships[slot].Type != MachineType.Airship)
            return false;

        airships[slot] = MachineInfo.None;
        Invoke();
        return true;
    }
}
