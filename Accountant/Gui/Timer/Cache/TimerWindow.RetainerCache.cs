using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Gui.Timer.Cache;
using Accountant.Timers;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed class RetainerCache : BaseCache
    {
        private readonly RetainerTimers _retainers;
        public           ObjectCounter  Counter;

        public RetainerCache(TimerWindow window, ConfigFlags requiredFlags, RetainerTimers retainers)
            : base("Retainers", requiredFlags, window)
        {
            _retainers         =  retainers;
            _retainers.Changed += Resetter;
        }

        private CacheObject GenerateRetainer(RetainerInfo retainer, ref ObjectCounter counter)
        {
            var type = counter.Add(retainer.Venture, Now, retainer.Available ? 10 : 0);
            return new CacheObject
            {
                Name          = retainer.Name,
                DisplayTime   = UpdateNextChange(retainer.Venture),
                Icon          = Icons.JobIcons[retainer.JobId],
                IconOffset    = 0.25f,
                DisplayString = Window.StatusString(type),
                Color         = retainer.Available ? ColorId.NeutralText : ColorId.DisabledText,
            };
        }

        private SmallHeader GeneratePlayer(string player, IEnumerable<RetainerInfo> retainers, ref ObjectCounter globalCount)
        {
            var local = ObjectCounter.Create();
            var newObject = new SmallHeader
            {
                Name         = player,
                ObjectsBegin = Objects.Count,
                Color        = ColorId.NeutralHeader,
                DisplayTime  = DateTime.MinValue,
            };

            Objects.AddRange(retainers
                .Where(r => r.RetainerId != 0)
                .Select(r => GenerateRetainer(r, ref local))
                .OrderByDescending(r => Accountant.Config.GetPriority(r.Name)));
            newObject.ObjectsCount =  Objects.Count - newObject.ObjectsBegin;
            newObject.Color        =  local.GetColorText();
            newObject.DisplayTime  =  local.GetTime();
            globalCount            += local;

            return newObject;
        }

        protected override void UpdateInternal()
        {
            Counter = ObjectCounter.Create();
            foreach (var (player, retainers) in _retainers.Data
                         .Where(r => !Accountant.Config.BlockedPlayersRetainers.Contains(r.Key.CastedName))
                         .Select(p => (GetName(p.Key.Name, p.Key.ServerId), p.Value))
                         .OrderByDescending(p => Accountant.Config.GetPriority(p.Item1)))
            {
                var p = GeneratePlayer(player, retainers, ref Counter);
                if (p.ObjectsCount == 0)
                    continue;

                Headers.Add(p);
            }

            Color       = Counter.GetColorHeader();
            DisplayTime = Counter.GetTime();
        }
    }
}