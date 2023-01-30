using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Game;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class TribeManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Tribes;

        private bool     _state;
        private DateTime _nextTribeCheck = DateTime.MinValue;

        private readonly TaskTimers _tasks;

        public TribeManager(TaskTimers tasks)
        {
            SignatureHelper.Initialise(this);
            _tasks = tasks;
            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => throw new NotImplementedException();

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

            _tasks.Reload();
            Dalamud.Framework.Update += OnFrameworkTribe;
            _state                   =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Framework.Update -= OnFrameworkTribe;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();

        private void UpdateTribes()
        {
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var allowances = (int) PlayerState.GetBeastTribeAllowance();
            if (allowances is < 0 or > Tribe.AllowanceCap)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer);

            if (_tasks!.AddOrUpdateTribes(player, allowances))
                _tasks.Save(player);
        }

        private void OnFrameworkTribe(Framework _)
        {
            var now = DateTime.UtcNow;
            if (_nextTribeCheck > now)
                return;

            UpdateTribes();
            _nextTribeCheck = now.AddMilliseconds(5535);
        }
    }
}
