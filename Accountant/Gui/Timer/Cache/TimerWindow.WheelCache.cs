using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Gui.Timer.Cache;
using Accountant.Timers;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed class WheelCache : BaseCache
    {
        private readonly WheelTimers   _wheels;
        public           ObjectCounter Counter;

        public WheelCache(TimerWindow window, ConfigFlags requiredFlags, WheelTimers wheels)
            : base("Aetherial Wheels", requiredFlags, window)
        {
            _wheels         =  wheels;
            _wheels.Changed += Resetter;
        }

        private CacheObject GenerateWheel(WheelInfo wheel, ref ObjectCounter local)
        {
            var (item, name, _) = Accountant.GameData.FindWheel(wheel.ItemId);
            var end = wheel.End();
            local.Add(end, Now, 6);
            return new CacheObject
            {
                Name          = name,
                DisplayTime   = UpdateNextChange(end),
                Icon          = item.Icon,
                IconOffset    = 0f,
                Color         = end < Now ? ColorId.TextObjectsHome : ColorId.NeutralText,
                DisplayString = end < Now ? "Primed" : null,
            };
        }

        private SmallHeader GenerateCompany(string company, IEnumerable<WheelInfo> wheels, ref ObjectCounter globalCount)
        {
            var local = ObjectCounter.Create();
            var newObject = new SmallHeader
            {
                Name         = company,
                ObjectsBegin = Objects.Count,
                Color        = ColorId.NeutralText,
                DisplayTime  = DateTime.MinValue,
            };
            Objects.AddRange(wheels
                .Where(w => w.ItemId != 0)
                .Select(w => GenerateWheel(w, ref local))
                .OrderByDescending(r => Accountant.Config.GetPriority(r.Name)));
            newObject.ObjectsCount = Objects.Count - newObject.ObjectsBegin;
            if (newObject.ObjectsCount == 0)
                return newObject;

            newObject.Color       =  local.GetColorText();
            newObject.DisplayTime =  local.GetTime();
            globalCount           += local;
            return newObject;
        }

        protected override void UpdateInternal()
        {
            Counter = ObjectCounter.Create();
            foreach (var (company, wheels) in _wheels.Data
                         .Where(r => !Accountant.Config.BlockedCompaniesWheels.Contains(r.Key.CastedName))
                         .Select(r => (GetName(r.Key.Name, r.Key.ServerId), r.Value))
                         .OrderByDescending(r => Accountant.Config.GetPriority(r.Item1)))
            {
                var fc = GenerateCompany(company, wheels, ref Counter);
                if (fc.ObjectsCount == 0)
                    continue;

                Headers.Add(fc);
            }

            Color       = Counter.GetColorHeader();
            DisplayTime = Counter.GetTime();
        }
    }
}
