using Accountant.Gui.Timer;
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

        public TaskManager(TaskTimers tasks)
        {
            _tasks = tasks;
            _subTasks = new ITimerManager[]
            {
                new LeveManager(tasks),
                new SquadronManager(tasks),
                new MapManager(tasks),
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
