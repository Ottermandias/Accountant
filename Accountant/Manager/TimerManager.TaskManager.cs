using System.Linq;
using Accountant.Gui.Timer;
using Accountant.SeFunctions;
using Accountant.Timers;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class TaskManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled;

        private readonly TaskTimers _tasks;

        private readonly ITimerManager[] _subTasks;

        private readonly UpdateGoldSaucerData _goldSaucerUpdate;

        public TaskManager(TaskTimers tasks)
        {
            _tasks            = tasks;
            _goldSaucerUpdate = new UpdateGoldSaucerData(Dalamud.Log, Dalamud.SigScanner);
            _subTasks = new ITimerManager[]
            {
                new LeveManager(tasks),
                new SquadronManager(tasks),
                new MapManager(tasks),
                new MiniCactpotManager(tasks, _goldSaucerUpdate),
                new JumboCactpotManager(tasks, _goldSaucerUpdate),
                new TribeManager(tasks),
                new DeliveryManager(tasks),
            };

            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => new TimerWindow.TaskCache(window, RequiredFlags, _tasks);

        public void SetState()
        {
            foreach (var manager in _subTasks)
                manager.SetState();
        }

        public void Dispose()
        {
            foreach (var manager in _subTasks)
                manager.Dispose();
        }
    }
}
