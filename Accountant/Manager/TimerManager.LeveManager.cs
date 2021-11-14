using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.SeFunctions;
using Accountant.Timers;
using Dalamud.Game;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class LeveManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.LeveAllowances;

        private readonly StaticLeveAllowances _leveAddress;
        private          bool                 _state;
        private          DateTime             _nextLeveCheck = DateTime.MinValue;

        private readonly TaskTimers _tasks;

        public LeveManager(TaskTimers tasks)
        {
            _tasks       = tasks;
            _leveAddress = new StaticLeveAllowances(Dalamud.SigScanner);
            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => throw new NotImplementedException();

        public void SetState()
        {
            if (Accountant.Config.Flags.Check(ConfigFlags.Enabled | ConfigFlags.LeveAllowances))
                Enable();
            else
                Disable();
        }

        private void Enable()
        {
            if (_state)
                return;

            _tasks.Reload();
            Dalamud.Framework.Update += OnFrameworkLeve;
            _state                   =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Framework.Update -= OnFrameworkLeve;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();

        private void UpdateLeves()
        {
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var leves = _leveAddress!.Leves();
            if (leves < 0)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer);

            if (_tasks!.AddOrUpdateLeves(player, leves))
                _tasks.Save(player);
        }

        private void OnFrameworkLeve(Framework _)
        {
            var now = DateTime.UtcNow;
            if (_nextLeveCheck > now)
                return;

            UpdateLeves();
            _nextLeveCheck = now.AddMilliseconds(7931);
        }
    }
}
