using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.SeFunctions;
using Accountant.Structs;
using Accountant.Timers;
using Dalamud.Game;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class RetainerManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Retainers;

        private readonly IntPtr   _retainerContainer;
        private          DateTime _nextRetainerCheck = DateTime.MinValue;
        private          bool     _state;

        private unsafe RetainerContainer* RetainerContainer
            => (RetainerContainer*)_retainerContainer;

        private readonly RetainerTimers _retainers;

        public RetainerManager(RetainerTimers retainers)
        {
            _retainers         = retainers;
            _retainerContainer = new StaticRetainerContainer(Dalamud.SigScanner).Address;

            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => new TimerWindow.RetainerCache(window, RequiredFlags, _retainers);

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

            _retainers.Reload();
            Dalamud.Framework.Update += OnFrameworkRetainer;
            _state                   =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Framework.Update -= OnFrameworkRetainer;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();

        private unsafe void UpdateRetainers()
        {
            if (Dalamud.ClientState.LocalPlayer == null || _retainerContainer == IntPtr.Zero || RetainerContainer->Ready != 1)
                return;

            var retainerList = (SeRetainer*)RetainerContainer->Retainers;

            var info    = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
            var count   = RetainerContainer->RetainerCount;
            var changes = false;
            for (byte i = 0; i < count; ++i)
            {
                var data = new RetainerInfo(retainerList[i]);
                changes |= _retainers.AddOrUpdateRetainer(info, data, i);
            }

            for (var i = count; i < RetainerInfo.MaxSlots; ++i)
                changes |= _retainers.ClearRetainer(info, i);

            if (changes)
                _retainers.Save(info);
        }

        private void OnFrameworkRetainer(Framework _)
        {
            var now = DateTime.UtcNow;
            if (_nextRetainerCheck > now)
                return;

            UpdateRetainers();
            _nextRetainerCheck = now.AddMilliseconds(5471);
        }
    }
}
