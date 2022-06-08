using System;
using System.Diagnostics.CodeAnalysis;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Timer;
using Accountant.Structs;
using Accountant.Timers;
using Accountant.Util;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class AirshipManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Airships;

        private readonly FreeCompanyStorage _companyStorage;

        private bool _state;

        private readonly AirshipTimers     _airships;
        private readonly SubmersibleTimers _submersibles;

        public AirshipManager(AirshipTimers airships, SubmersibleTimers submersibles, FreeCompanyStorage companyStorage)
        {
            SignatureHelper.Initialise(this);
            _airships       = airships;
            _submersibles   = submersibles;
            _companyStorage = companyStorage;
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

            _airshipTimersHook?.Enable();
            _airshipStatusListHook?.Enable();
            _airships.Reload();
            _state = true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            _airshipTimersHook?.Disable();
            _airshipStatusListHook?.Disable();
            _state = false;
        }

        public void Dispose()
        {
            Disable();
            _airshipTimersHook?.Dispose();
            _airshipStatusListHook?.Dispose();
        }

        private delegate void PacketHandler(IntPtr manager, IntPtr data);

        [Signature("E8 ?? ?? ?? ?? 33 D2 48 8D 4C 24 ?? 41 B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 54 24 ?? 48 8B CB E8 ?? ?? ?? ?? 48 8B 3D",
            DetourName = nameof(AirshipTimersDetour))]
        private Hook<PacketHandler>? _airshipTimersHook = null!;

        [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8D 99 ?? ?? ?? ?? C6 81",
            DetourName = nameof(AirshipStatusListDetour))]
        private Hook<PacketHandler>? _airshipStatusListHook = null!;

        private bool FreeCompanyInfo([NotNullWhen(true)] ref FreeCompanyInfo? info)
        {
            if (info != null)
                return true;

            info = _companyStorage.GetCurrentCompanyInfo();
            if (info != null)
                return true;

            PluginLog.Error("Could not log airships, unable to obtain free company name.");
            return false;
        }

        private unsafe void AirshipTimersDetour(IntPtr manager, IntPtr data)
        {
            try
            {
                FreeCompanyInfo? info    = null;
                var              changes = false;
                var              timer   = (AirshipTimer*)data;
                for (byte i = 0; i < 4; ++i)
                {
                    if (timer[i].RawName[0] == 0)
                        break;

                    if (!FreeCompanyInfo(ref info))
                        return;

                    changes |= _airships.AddOrUpdateAirship(info.Value, new MachineInfo(timer[i].Name, timer[i].Date, MachineType.Airship), i);
                }

                if (changes)
                    _airships.Save(info!.Value);
            }
            finally
            {
                _airshipTimersHook!.Original(manager, data);
            }
        }

        private unsafe void AirshipStatusListDetour(IntPtr manager, IntPtr data)
        {
            try
            {
                FreeCompanyInfo? info    = null;
                var              changes = false;
                var              status  = (AirshipStatus*)data;
                for (byte i = 0; i < 4; ++i)
                {
                    if (status[i].RawName[0] == 0)
                        break;

                    if (!FreeCompanyInfo(ref info))
                        return;

                    changes |= _airships.AddOrUpdateAirship(info.Value, new MachineInfo(status[i].Name, status[i].Date, MachineType.Airship),
                        i);
                }

                if (changes)
                    _airships.Save(info!.Value);
            }
            finally
            {
                _airshipStatusListHook!.Original(manager, data);
            }
        }
    }
}
