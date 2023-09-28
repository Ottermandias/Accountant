using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

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
            Dalamud.Interop.InitializeFromAttributes(this);
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

        private unsafe void UpdateTribes()
        {
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var allowances = (int) QuestManager.Instance()->GetBeastTribeAllowance();
            if (allowances is < 0 or > Tribe.AllowanceCap)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer);

            if (_tasks!.AddOrUpdateTribes(player, allowances))
                _tasks.Save(player);
        }

        private void OnFrameworkTribe(IFramework _)
        {
            var now = DateTime.UtcNow;
            if (_nextTribeCheck > now)
                return;

            UpdateTribes();
            _nextTribeCheck = now.AddMilliseconds(5535);
        }
    }
}
