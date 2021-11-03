using System;
using Accountant.Classes;
using Accountant.SeFunctions;
using Accountant.Structs;
using Accountant.Timers;
using Dalamud.Game;


namespace Accountant.Manager;

public unsafe partial class TimerManager
{
    private IntPtr   _retainers = IntPtr.Zero;
    private DateTime _nextCheck = DateTime.MinValue;

    private RetainerContainer* Retainers
        => (RetainerContainer*)_retainers;

    public void EnableRetainers(bool state = true, bool force = false)
    {
        if (!force && state == Accountant.Config.EnableRetainers)
            return;

        if (state)
        {
            if (_retainers == IntPtr.Zero)
                _retainers = new StaticRetainerContainer(Dalamud.SigScanner).Address;

            RetainerTimers           ??= RetainerTimers.Load();
            Dalamud.Framework.Update +=  OnFramework;
        }
        else
        {
            Dalamud.Framework.Update -= OnFramework;
            RetainerTimers           =  null;
        }

        if (!force)
        {
            Accountant.Config.EnableRetainers = state;
            Accountant.Config.Save();
        }
    }

    public void UpdateRetainers()
    {
        if (Dalamud.ClientState.LocalPlayer == null || _retainers == IntPtr.Zero || Retainers->Ready != 1)
            return;

        var retainerList = (SeRetainer*)Retainers->Retainers;

        var info    = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
        var count   = Retainers->RetainerCount;
        var changes = false;
        for (byte i = 0; i < count; ++i)
        {
            var data = new RetainerInfo(retainerList[i]);
            changes |= RetainerTimers!.AddOrUpdateRetainer(info, data, i);
        }

        for (var i = count; i < RetainerInfo.MaxSlots; ++i)
            changes |= RetainerTimers!.RemoveRetainer(info, i);

        if (changes)
            RetainerTimers!.Save(info);
    }

    private void OnFramework(Framework _)
    {
        var now = DateTime.UtcNow;
        if (_nextCheck > now)
            return;

        UpdateRetainers();
        _nextCheck = now.AddSeconds(5);
    }
}
