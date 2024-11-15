using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Plugin.Services;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class SquadronManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Squadron;

        private          bool                    _state;
        private          DateTime                _nextSquadronCheck = DateTime.MinValue;

        private readonly TaskTimers _tasks;

        public SquadronManager(TaskTimers tasks)
        {
            _tasks    = tasks;
            SetState();
        }

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
            Dalamud.Framework.Update += OnFrameworkSquadron;
            _state                   =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Framework.Update -= OnFrameworkSquadron;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => throw new NotImplementedException();

        private void UpdateSquadron()
        {
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var missionId = Interop.SquadronContainer.MissionId;
            if (missionId == ushort.MaxValue)
                return;

            var info = new Squadron
            {
                MissionId   = missionId,
                TrainingId  = Interop.SquadronContainer.TrainingId,
                MissionEnd  = Interop.SquadronContainer.MissionEnd,
                TrainingEnd = Interop.SquadronContainer.TrainingEnd,
                NewRecruits = Interop.SquadronContainer.NewRecruits,
            };

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer);

            if (_tasks.AddOrUpdateSquadron(player, info))
                _tasks.Save(player);
        }

        private void OnFrameworkSquadron(IFramework _)
        {
            var now = DateTime.UtcNow;
            if (_nextSquadronCheck > now)
                return;

            UpdateSquadron();
            _nextSquadronCheck = now.AddMilliseconds(6331);
        }
    }
}
