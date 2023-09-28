using System;
using Accountant.Classes;
using Accountant.Util;
using Dalamud.Logging;

namespace Accountant.Timers;


public sealed class WheelTimers : TimersBase<FreeCompanyInfo, WheelInfo[]>
{
    protected override string FolderName
        => "wheels";

    protected override string SaveError
        => "Could not write aetherial wheel data";

    protected override string ParseError
        => "Invalid aetherial wheel data file could not be parsed";

    protected override string LoadError
        => "Error loading aetherial wheel timers";

    public bool AddOrUpdateWheel(FreeCompanyInfo company, WheelInfo wheel, byte slot)
    {
        --slot;
        if (slot >= WheelInfo.MaxSlots)
        {
            Dalamud.Log.Error($"Only {WheelInfo.MaxSlots} aetherial wheels supported.");
            return false;
        }

        if (wheel.ItemId == 0)
            return false;

        if (!InternalData.TryGetValue(company, out var wheels))
        {
            wheels                = WheelInfo.GenerateDefaultArray();
            wheels[slot]          = wheel;
            InternalData[company] = wheels;
            Invoke();
            return true;
        }

        var oldWheel = wheels[slot];
        if (oldWheel.ItemId != wheel.ItemId)
        {
            wheels[slot] = wheel;
            Invoke();
            return true;
        }

        if (Helpers.DateTimeClose(oldWheel.Placed, wheel.Placed))
            return false;

        if (oldWheel.Accurate && !wheel.Accurate && Math.Abs((oldWheel.Placed - wheel.Placed).TotalHours) < 1)
            return false;

        if (wheel.Placed == DateTime.MinValue && oldWheel.End() < DateTime.Now)
            return false;

        wheels[slot] = wheel;
        Invoke();
        return true;
    }

    public bool RemoveWheel(FreeCompanyInfo company, byte slot)
    {
        --slot;
        if (slot >= WheelInfo.MaxSlots)
        {
            Dalamud.Log.Error($"Only {WheelInfo.MaxSlots} aetherial wheels supported.");
            return false;
        }

        if (!InternalData.TryGetValue(company, out var wheels))
            return false;

        if (wheels[slot].ItemId == 0)
            return false;
        wheels[slot] = WheelInfo.None;
        Invoke();
        return true;
    }
}
