using System;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Game;
using Dalamud.Utility.Signatures;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed unsafe class DeliveryManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.CustomDelivery;

        [Signature(Signatures.CustomDeliveryData, ScanType = ScanType.StaticAddress)]
        private readonly IntPtr _customDeliveryData = IntPtr.Zero;

        [Signature(Signatures.CustomDeliveryAllowances)]
        private readonly delegate* unmanaged<IntPtr, int> _getAllowances = null!;

        private bool     _state;
        private DateTime _nextDeliveryCheck = DateTime.MinValue;

        private readonly TaskTimers _tasks;

        public DeliveryManager(TaskTimers tasks)
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
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var allowances = Delivery.AllowanceCap - _getAllowances(_customDeliveryData);
            if (allowances is < 0 or > Delivery.AllowanceCap)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer);

            if (_tasks!.AddOrUpdateDeliveries(player, allowances))
                _tasks.Save(player);
        }

        private void OnFrameworkDelivery(Framework _)
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
