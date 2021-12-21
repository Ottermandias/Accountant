using System;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Timer;
using Accountant.Structs;
using Accountant.Timers;
using Accountant.Util;
using Dalamud.Game.Network;
using Dalamud.Logging;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class SubmersibleManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Submersibles;

        private readonly FreeCompanyStorage _companyStorage;

        private bool   _state;
        private ushort SubmarineTimerOpCode  { get; }
        private ushort SubmarineStatusOpCode { get; }

        private readonly SubmersibleTimers _submersibles;
        private readonly AirshipTimers     _airships;

        public SubmersibleManager(SubmersibleTimers submersibles, AirshipTimers airships, FreeCompanyStorage companyStorage)
        {
            _submersibles   = submersibles;
            _airships       = airships;
            _companyStorage = companyStorage;

            SubmarineTimerOpCode =
                (ushort)(Dalamud.GameData.ServerOpCodes.TryGetValue("SubmarineTimers", out var code) ? code : 0x006C); // 6.01
            SubmarineStatusOpCode =
                (ushort)(Dalamud.GameData.ServerOpCodes.TryGetValue("SubmarineStatusList", out code) ? code : 0x010E); // 6.01

            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => new TimerWindow.MachineCache(window, RequiredFlags, "Submersibles", MachineType.Submersible, _airships, _submersibles);

        public void SetState()
        {
            if (Accountant.Config.Flags.Check(RequiredFlags))
                Enable();
            else
                Disable();
        }

        private void Enable()
        {
            if (_state)
                return;

            _submersibles.Reload();
            Dalamud.Network.NetworkMessage += NetworkMessage;
            _state                         =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Network.NetworkMessage -= NetworkMessage;
            _state                         =  false;
        }

        public void Dispose()
            => Disable();

        private unsafe void NetworkMessage(IntPtr data, ushort opCode, uint sourceId, uint targetId, NetworkMessageDirection direction)
        {
            FreeCompanyInfo? info = null;

            bool SetCompanyInfo()
            {
                if (info != null)
                    return false;

                info = _companyStorage.GetCurrentCompanyInfo();
                if (info != null)
                    return false;

                PluginLog.Error("Could not log submersibles, unable to obtain free company name.");
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

                    changes |= _submersibles.AddOrUpdateSubmersible(info!.Value,
                        new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Submersible), i);
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

                    changes |= _submersibles.AddOrUpdateSubmersible(info!.Value,
                        new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Submersible), i);
                }
            }

            if (changes)
                _submersibles.Save(info!.Value);
        }
    }
}
