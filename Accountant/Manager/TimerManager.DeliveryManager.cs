using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed unsafe class DeliveryManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.CustomDelivery;

        private bool     _state;
        private DateTime _nextDeliveryCheck = DateTime.MinValue;

        private readonly TaskTimers _tasks;

        public DeliveryManager(TaskTimers tasks)
        {
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
            Dalamud.Framework.Update += OnFrameworkDelivery;
            _state                   =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Framework.Update -= OnFrameworkDelivery;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();

        private void UpdateDeliveries()
        {
            if (Dalamud.Objects.LocalPlayer is not {} p)
                return;

            var allowances = Delivery.AllowanceCap - SatisfactionSupplyManager.Instance()->GetUsedAllowances();
            if (allowances is < 0 or > Delivery.AllowanceCap)
                return;

            var player = new PlayerInfo(p);

            if (_tasks!.AddOrUpdateDeliveries(player, allowances))
                _tasks.Save(player);
        }

        private void OnFrameworkDelivery(IFramework _)
        {
            var now = DateTime.UtcNow;
            if (_nextDeliveryCheck > now)
                return;

            UpdateDeliveries();
            _nextDeliveryCheck = now.AddMilliseconds(3779);
        }
    }
}

public partial class TimerManager
{ }
