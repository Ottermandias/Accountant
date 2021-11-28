using System;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Timer;
using Accountant.SeFunctions;
using Accountant.Timers;
using AddonWatcher;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using OtterLoc.Structs;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class JumboCactpotManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.JumboCactpot;

        private readonly TaskTimers _tasks;

        private          bool                            _state;
        private readonly IAddonWatcher                   _watcher;
        private readonly Hook<UpdateGoldSaucerDelegate>? _goldSaucerUpdateHook;

        public JumboCactpotManager(TaskTimers tasks, UpdateGoldSaucerData goldSaucerUpdate)
        {
            _tasks                = tasks;
            _watcher              = Accountant.Watcher;
            _goldSaucerUpdateHook = goldSaucerUpdate.CreateHook(UpdateGoldSaucerDetour, false);
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

            _goldSaucerUpdateHook?.Enable();
            _watcher.SubscribeYesnoSelected(TicketBought);
            _watcher.SubscribeLotteryWeeklyRewardListSetup(RewardSetupDetour);
            _tasks.Reload();
            _state = true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            _goldSaucerUpdateHook?.Disable();
            _watcher.UnsubscribeYesnoSelected(TicketBought);
            _watcher.UnsubscribeLotteryWeeklyRewardListSetup(RewardSetupDetour);
            _state = false;
        }

        public void Dispose()
        {
            Disable();
            _goldSaucerUpdateHook?.Dispose();
        }

        private void RewardSetupDetour(IntPtr _)
        {
            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
            if (_tasks.ClearFirstJumbo(player))
                _tasks.Save(player);
        }

        private unsafe void UpdateGoldSaucerDetour(IntPtr _, IntPtr packet)
        {
            var jumbo = new JumboCactpot()
            {
                LastUpdate = DateTime.UtcNow,
            };

            var statePtr = (byte*)(packet + 0x1E);
            var ptr      = (ushort*)(packet + 0x22);
            var tickets  = jumbo.Tickets;
            for (var i = 0; i < JumboCactpot.MaxTickets; ++i)
            {
                if (statePtr[i] >= 3 || statePtr[i] == 0)
                    continue;
                var ticket = ptr[i];
                if (ticket >= 10000)
                    ticket = 0xFFFF;
                tickets[i] = ticket;
            }

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
            if (statePtr[0] == 2)
                jumbo.LastUpdate = jumbo.NextReset(player.ServerId).AddDays(-7);
            if (_tasks.AddOrUpdateJumboCactpot(player, jumbo))
                _tasks.Save(player);
            _goldSaucerUpdateHook!.Original(_, packet);
        }

        private void TicketBought(IntPtr _, bool which, SeString button, SeString description)
        {
            if (!which)
                return;

            var desc = description.ToString();
            if (!StringId.BuyJumboCactpotTicket.Match(desc))
                return;

            var numbers = StringId.FilterJumboCactpotTicket.Filter(desc);
            if (numbers.Count == 0)
                return;
            if (!ushort.TryParse(numbers[0], out var ticket) || ticket >= 10000)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
            if (_tasks.AddOrUpdateJumbo(player, ticket))
                _tasks.Save(player);
        }
    }
}
