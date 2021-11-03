using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Manager;
using OtterLoc.Structs;

namespace Accountant.Gui;

public partial class TimerWindow
{
    private sealed class RetainerCache : ObjectCache
    {
        public RetainerCache(TimerWindow window, TimerManager manager)
            : base(window, manager)
        {
            Resubscribe();
        }

        private void Resubscribe()
        {
            if (Manager.RetainerTimers != null)
                Manager.RetainerTimers.RetainerChanged += Resetter;
        }

        private CacheObject GenerateRetainer(RetainerInfo retainer)
        {
            var ret = new CacheObject()
            {
                Name          = retainer.Name,
                Children      = Array.Empty<CacheObject>(),
                DisplayTime   = retainer.Venture,
                Icon          = Window._icons[Icons.JobIcons[retainer.JobId]],
                IconOffset    = 0.25f,
                DisplayString = GetDisplayInfo(retainer.Venture),
                Color         = retainer.Available ? ColorId.NeutralText : ColorId.DisabledText,
            };
            if (!retainer.Available)
            {
                ++CurrentLimitedObjects;
                --CurrentAvailableObjects;
            }

            return ret;
        }

        private CacheObject GeneratePlayer(PlayerInfo player, IEnumerable<RetainerInfo> retainers)
        {
            ResetCurrent();
            var newObject = new CacheObject()
            {
                Name     = GetName(player.Name, player.ServerId),
                Children = retainers.Where(r => r.RetainerId != 0).Select(GenerateRetainer).ToArray(),
            };
            if (newObject.Children.Length == 0)
                return newObject;

            if (CurrentSentObjects == newObject.Children.Length)
            {
                newObject.Color       = ColorId.TextObjectsAway;
                newObject.DisplayTime = CurrentTimeForFirst;
            }
            else if (CurrentSentObjects > 0)
            {
                newObject.Color       = ColorId.TextObjectsMixed;
                newObject.DisplayTime = CurrentTimeForAll;
            }
            else
            {
                newObject.Color         = CurrentCompletedObjects > 0 ? ColorId.TextObjectsHome : ColorId.NeutralText;
                newObject.DisplayString = string.Empty;
            }

            AddCurrent();

            return newObject;
        }

        private void SetGlobals()
        {
            Header = $"{StringId.Retainers.Value()}: {CompletedObjects} | {AvailableObjects} | {SentObjects}";
            if (SentObjects == TotalObjects - LimitedObjects)
            {
                GlobalColor = ColorId.HeaderObjectsAway;
                GlobalTime  = TimeForFirst;
            }
            else if (SentObjects > 0)
            {
                GlobalColor = ColorId.HeaderObjectsMixed;
                GlobalTime  = TimeForAll;
            }
            else
            {
                GlobalColor = ColorId.HeaderObjectsHome;
                GlobalTime  = DateTime.MinValue;
            }
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();
            foreach (var (player, retainers) in Manager.RetainerTimers!.Retainers)
            {
                var p = GeneratePlayer(player, retainers);
                if (p.Children.Length == 0)
                    continue;

                Objects.Add(p);
            }

            SetGlobals();
        }
    }
}
