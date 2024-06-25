using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Plugin.Services;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class RetainerManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Retainers;

        private DateTime _nextRetainerCheck = DateTime.MinValue;
        private bool     _state;

        private readonly RetainerTimers _retainers;

        public RetainerManager(RetainerTimers retainers)
        {
            _retainers = retainers;
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
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var manager = FFXIVClientStructs.FFXIV.Client.Game.RetainerManager.Instance();
            if (manager == null || manager->Ready != 1)
                return;

            var retainerList = manager->Retainers;
            var info         = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
            var count        = manager->GetRetainerCount();
            var changes      = false;
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

        private void OnFrameworkRetainer(IFramework _)
        {
            var now = DateTime.UtcNow;
            if (_nextRetainerCheck > now)
                return;

            UpdateRetainers();
            _nextRetainerCheck = now.AddMilliseconds(5471);
        }
    }
}
