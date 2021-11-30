using System;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Timer;
using Accountant.Timers;
using Accountant.Util;
using AddonWatcher;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using OtterLoc.Structs;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class WheelManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.AetherialWheels;

        private readonly IAddonWatcher      _watcher;
        private readonly FreeCompanyStorage _companyStorage;

        private          bool        _state;
        private readonly WheelTimers _wheels;

        public WheelManager(WheelTimers wheels, FreeCompanyStorage companyStorage)
        {
            _wheels         = wheels;
            _watcher        = Accountant.Watcher;
            _companyStorage = companyStorage;

            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => new TimerWindow.WheelCache(window,RequiredFlags,  _wheels);

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

            _wheels.Reload();
            Dalamud.Framework.Update += OnFrameworkWheel;
            _watcher.SubscribeYesnoSelected(WheelOnYesNo);
            _state = true;
        }

        private void Disable()
        {
            if (!_state)
                return;

            _watcher.UnsubscribeYesnoSelected(WheelOnYesNo);
            Dalamud.Framework.Update -= OnFrameworkWheel;
            _state                   =  false;
        }

        public void Dispose()
            => Disable();

        private static unsafe byte ActiveWheelSlot()
        {
            var wheel = (AtkUnitBase*)Dalamud.GameGui.GetAddonByName("AetherialWheel", 1);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (wheel == null || wheel->UldManager.NodeListCount < 14)
                return 0;

            for (var i = 8; i < 8 + WheelInfo.MaxSlots; ++i)
            {
                var button = (AtkComponentNode*)wheel->UldManager.NodeList[i];
                if (button == null || button->Component->UldManager.NodeListCount < 3)
                    continue;

                if (button->Component->UldManager.NodeList[2]->IsVisible)
                    return (byte)(14 - i);
            }

            return 0;
        }

        private void WheelOnYesNo(IntPtr _, bool which, SeString button, SeString description)
        {
            if (!which)
                return;

            var newDesc = new SeString(description.Payloads.Where(p => p is not NewLinePayload).ToList());
            var ret     = StringId.WheelFilter.Filter(newDesc);
            if (ret.Count == 0)
                return;

            var slot = ActiveWheelSlot();
            if (slot == 0)
                return;

            var (item, _, grade) = Accountant.GameData.FindWheel(ret[0]);
            if (grade == 0)
                return;

            var info = new WheelInfo
            {
                Accurate = true,
                Grade    = grade,
                ItemId   = item.RowId,
                Placed   = DateTime.UtcNow,
            };

            var fc = _companyStorage.GetCurrentCompanyInfo();
            if (fc == null)
            {
                PluginLog.Error("Could not log wheel, unable to obtain free company name.");
                return;
            }

            if (_wheels!.AddOrUpdateWheel(fc.Value, info, slot))
                _wheels.Save(fc.Value);
        }

        private unsafe void OnFrameworkWheel(Framework _)
        {
            var wheel = (AtkUnitBase*)Dalamud.GameGui.GetAddonByName("AetherialWheel", 1);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (wheel == null || wheel->UldManager.NodeListCount < 14)
                return;

            FreeCompanyInfo? fc = null;

            bool SetCompanyInfo()
            {
                if (fc != null)
                    return false;

                fc = _companyStorage.GetCurrentCompanyInfo();
                if (fc != null)
                    return false;

                PluginLog.Error("Could not log machines, unable to obtain free company name.");
                return true;
            }

            var change = false;
            for (var i = 8; i < 8 + WheelInfo.MaxSlots; ++i)
            {
                var button = (AtkComponentNode*)wheel->UldManager.NodeList[i];
                if (button == null || button->Component->UldManager.NodeListCount < 10)
                    continue;

                if (SetCompanyInfo())
                    return;

                var text = button->Component->UldManager.NodeList[5];
                var now  = DateTime.UtcNow;
                var slot = (byte)(14 - i);
                if (!text->IsVisible)
                {
                    change |= _wheels.RemoveWheel(fc!.Value, slot);
                }
                else
                {
                    var fill     = button->Component->UldManager.NodeList[8]->ScaleX;
                    var seString = MemoryHelper.ReadSeString(&((AtkTextNode*)button->Component->UldManager.NodeList[9])->NodeText);
                    seString.Payloads.RemoveAll(p => p is NewLinePayload);
                    var name = seString.TextValue;
                    var (item, _, grade) = Accountant.GameData.FindWheel(name);
                    if (grade == 0)
                        continue;

                    var info = new WheelInfo()
                    {
                        Accurate = false,
                        ItemId   = item.RowId,
                        Grade    = grade,
                        Placed   = fill >= 0.9999 ? DateTime.MinValue : now.AddHours(-WheelInfo.HoursType(grade) * fill),
                    };
                    change |= _wheels.AddOrUpdateWheel(fc!.Value, info, slot);
                }
            }

            if (change)
                _wheels.Save(fc!.Value);
        }
    }
}
