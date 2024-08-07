using System;
using System.Linq;
using Accountant.Classes;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class MapManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.MapAllowance;

        private bool _state;

        private readonly IGameData  _gameData;
        private readonly TaskTimers _tasks;
        private          DateTime   _nextMapCheck = DateTime.MinValue;

        public MapManager(TaskTimers tasks)
        {
            _tasks    = tasks;
            _gameData = Accountant.GameData;
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
            Dalamud.Chat.CheckMessageHandled += OnChatMap;
            Dalamud.Framework.Update         += OnFrameworkMap;
            _state                           =  true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            Dalamud.Chat.CheckMessageHandled -= OnChatMap;
            Dalamud.Framework.Update -= OnFrameworkMap;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();


        private void OnChatMap(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if ((int)type != 2115 || !Dalamud.Conditions[ConditionFlag.Gathering])
                return;

            var item = (ItemPayload?)message.Payloads.FirstOrDefault(p => p is ItemPayload);
            if (item == null)
                return;

            if (_gameData.FindMap(item.ItemId) == null)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer!);
            if (_tasks!.AddOrUpdateMap(player, DateTime.UtcNow.AddHours(18)))
                _tasks.Save(player);
        }


        private unsafe void UpdateMap()
        {
            if (Dalamud.ClientState.LocalPlayer == null)
                return;

            var uiState = UIState.Instance();
            if (uiState == null)
                return;

            var time = uiState->GetNextMapAllowanceDateTime();
            if (time == DateTime.MaxValue || time == DateTime.MinValue)
                return;

            var player = new PlayerInfo(Dalamud.ClientState.LocalPlayer);

            if (_tasks!.AddOrUpdateMap(player, time))
                _tasks.Save(player);
        }

        private void OnFrameworkMap(IFramework _)
        {
            var now = DateTime.UtcNow;
            if (_nextMapCheck > now)
                return;

            UpdateMap();
            _nextMapCheck = now.AddMilliseconds(9173);
        }
    }
}
