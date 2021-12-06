using System;
using System.Reflection.Metadata.Ecma335;
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
    private sealed class AirshipManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Airships;

        private readonly FreeCompanyStorage _companyStorage;

        private bool   _state;
        private ushort AirshipTimerOpCode  { get; }
        private ushort AirshipStatusOpCode { get; }

        private readonly AirshipTimers     _airships;
        private readonly SubmersibleTimers _submersibles;

        public AirshipManager(AirshipTimers airships, SubmersibleTimers submersibles, FreeCompanyStorage companyStorage)
        {
            _airships       = airships;
            _submersibles   = submersibles;
            _companyStorage = companyStorage;

            AirshipTimerOpCode  = 0x00ED; // Dalamud.GameData.ServerOpCodes["AirshipTimers"]
            AirshipStatusOpCode = 0x023F; // Dalamud.GameData.ServerOpCodes["AirshipStatusList"]
            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => new TimerWindow.MachineCache(window, RequiredFlags, "Airships", MachineType.Airship, _airships, _submersibles);

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

            _airships.Reload();
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

                PluginLog.Error("Could not log airships, unable to obtain free company name.");
                return true;
            }

            var changes = false;
            if (opCode == AirshipTimerOpCode)
            {
                var timer = (AirshipTimer*)data;
                for (byte i = 0; i < 4; ++i)
                {
                    if (timer[i].RawName[0] == 0)
                        break;

                    if (SetCompanyInfo())
                        return;

                    changes |= _airships.AddOrUpdateAirship(info!.Value, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Airship), i);
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

                    changes |= _airships.AddOrUpdateAirship(info!.Value, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Airship), i);
                }
            }

            if (changes)
                _airships.Save(info!.Value);
        }
    }
}
