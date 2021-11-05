using System;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Structs;
using Accountant.Util;
using Dalamud.Game.Network;
using Dalamud.Logging;

namespace Accountant.Manager;

public partial class TimerManager
{
    private FreeCompanyStorage? _companyStorage;

    public ushort AirshipTimerOpCode    { get; private set; }
    public ushort AirshipStatusOpCode   { get; private set; }
    public ushort SubmarineTimerOpCode  { get; private set; }
    public ushort SubmarineStatusOpCode { get; private set; }

    public void EnableMachines(bool state = true, bool force = false)
    {
        if (!force && state == Accountant.Config.EnableMachines)
            return;

        if (state)
        {
            _companyStorage                ??= FreeCompanyStorage.Load();
            MachineTimers                  ??= Timers.MachineTimers.Load();
            Dalamud.Network.NetworkMessage +=  NetworkMessage;
        }
        else
        {
            Dalamud.Network.NetworkMessage -= NetworkMessage;
            MachineTimers                  =  null;
            _companyStorage                =  null;
        }

        if (!force)
        {
            Accountant.Config.EnableMachines = state;
            Accountant.Config.Save();
        }
    }

    private void SetupOpCodes()
    {
        AirshipTimerOpCode    = 0x0166; // Dalamud.GameData.ServerOpCodes["AirshipTimers"]
        AirshipStatusOpCode   = 0x02FE; // Dalamud.GameData.ServerOpCodes["AirshipStatusList"]
        SubmarineTimerOpCode  = 0x0247; // Dalamud.GameData.ServerOpCodes["SubmarineTimers"]
        SubmarineStatusOpCode = 0x01EF; // Dalamud.GameData.ServerOpCodes["SubmarineStatusList"]
    }

    private FreeCompanyInfo? GetCompanyInfo()
    {
        if (Dalamud.ClientState.LocalPlayer == null)
            return null;

        var (tag, name, leader) = _gameData.FreeCompanyInfo();
        var id = Dalamud.ClientState.LocalPlayer.HomeWorld.Id;
        return _companyStorage!.FindByAndUpdateInfo(tag, name, leader, id);
    }

    private unsafe void NetworkMessage(IntPtr data, ushort opCode, uint sourceId, uint targetId, NetworkMessageDirection direction)
    {
        if (!_gameData.Valid)
            return;

        FreeCompanyInfo? info = null;

        bool SetCompanyInfo()
        {
            if (info != null)
                return false;

            info = GetCompanyInfo();
            if (info != null)
                return false;

            PluginLog.Error("Could not log machines, unable to obtain free company name.");
            return true;
        }

        var changes = false;
        if (opCode == SubmarineTimerOpCode)
        {
            var timer = (SubmersibleTimer*)data;
            for (byte i = 0; i < 4; ++i)
            {
                if (timer[i].RawName[0] == 0)
                    break;

                if (SetCompanyInfo())
                    return;

                changes |= MachineTimers!.AddOrUpdateMachine(info!, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Submersible), i);
            }
        }
        else if (opCode == AirshipTimerOpCode)
        {
            var timer = (AirshipTimer*)data;
            for (byte i = 0; i < 4; ++i)
            {
                if (timer[i].RawName[0] == 0)
                    break;

                if (SetCompanyInfo())
                    return;

                changes |= MachineTimers!.AddOrUpdateMachine(info!, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Airship), i);
            }
        }
        else if (opCode == SubmarineStatusOpCode)
        {
            var timer = (SubmersibleStatus*)data;
            for (byte i = 0; i < 4; ++i)
            {
                if (timer[i].RawName[0] == 0)
                    break;

                if (SetCompanyInfo())
                    return;

                changes |= MachineTimers!.AddOrUpdateMachine(info!, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Submersible), i);
            }
        }
        else if (opCode == AirshipStatusOpCode)
        {
            var timer = (AirshipStatus*)data;
            for (byte i = 0; i < 4; ++i)
            {
                if (timer[i].RawName[0] == 0)
                    break;

                if (SetCompanyInfo())
                    return;

                changes |= MachineTimers!.AddOrUpdateMachine(info!, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Airship), i);
            }
        }

        if (changes)
            MachineTimers!.Save(info!);
    }
}
