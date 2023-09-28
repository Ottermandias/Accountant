using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Timers;
using Dalamud.Interface.Internal;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed class MachineCache : BaseCache
    {
        private readonly AirshipTimers     _airships;
        private readonly SubmersibleTimers _submersibles;
        private readonly MachineType       _type;
        public           string            Header = string.Empty;

        private readonly IDalamudTextureWrap _icon;
        public           ObjectCounter       GlobalCounter;

        public MachineCache(TimerWindow window, ConfigFlags requiredFlags, string name, MachineType type, AirshipTimers airships,
            SubmersibleTimers submersibles)
            : base(name, requiredFlags, window)
        {
            _airships             =  airships;
            _submersibles         =  submersibles;
            _airships.Changed     += Resetter;
            _submersibles.Changed += Resetter;
            _type                 =  type;
            _icon                 =  Window._icons[_type == MachineType.Airship ? Icons.AirshipIcon : Icons.SubmarineIcon];
        }

        private CacheObject GenerateMachine(MachineInfo machine, ref ObjectCounter counter)
        {
            var type = counter.Add(machine.Arrival, Now, MachineInfo.MaxSlots);
            return new CacheObject
            {
                Name          = machine.Name,
                DisplayTime   = UpdateNextChange(machine.Arrival),
                Icon          = _icon,
                IconOffset    = 0.125f,
                DisplayString = Window.StatusString(type),
                Color         = type == ObjectStatus.Limited ? ColorId.DisabledText : ColorId.NeutralText,
            };
        }

        private SmallHeader GenerateCompany(string name, FreeCompanyInfo company, IEnumerable<MachineInfo> data, ref ObjectCounter global)
        {
            var begin = Objects.Count;

            var localMain = ObjectCounter.Create();
            Objects.AddRange(data
                .Where(a => a.Type != MachineType.Unknown)
                .Select(a => GenerateMachine(a, ref localMain))
                .OrderByDescending(a => Accountant.Config.GetPriority(a.Name)));

            var localOff = ObjectCounter.Create();
            switch (_type)
            {
                case MachineType.Submersible when Accountant.Config.Flags.Check(ConfigFlags.Airships)
                 && _airships.Data.TryGetValue(company, out var airships):
                {
                    foreach (var airship in airships)
                        localOff.Add(UpdateNextChange(airship.Arrival), Now, MachineInfo.MaxSlots);
                    break;
                }
                case MachineType.Airship when Accountant.Config.Flags.Check(ConfigFlags.Submersibles)
                 && _submersibles.Data.TryGetValue(company, out var submersibles):
                {
                    foreach (var submersible in submersibles)
                        localOff.Add(UpdateNextChange(submersible.Arrival), Now, MachineInfo.MaxSlots);
                    break;
                }
            }

            if ((localMain + localOff).VerifyLimit(MachineInfo.MaxSlots))
            {
                localMain.VerifyLimit(0);
                var end             = Objects.Count;
                var availableString = Window.StatusString(ObjectStatus.Available);
                for (var i = begin; i < end; ++i)
                {
                    var o = Objects[i];
                    if (o.DisplayString != availableString)
                        continue;

                    o.DisplayString = Window.StatusString(ObjectStatus.Limited);
                    o.Color         = ColorId.DisabledText;
                    Objects[i]      = o;
                }
            }

            global += localMain;

            return new SmallHeader
            {
                Name         = name,
                ObjectsBegin = begin,
                ObjectsCount = Objects.Count - begin,
                DisplayTime  = localMain.GetTime(),
                Color        = localMain.GetColorText(),
            };
        }

        protected override void UpdateInternal()
        {
            GlobalCounter = ObjectCounter.Create();
            foreach (var (name, company, data) in (_type == MachineType.Airship ? _airships.Data : _submersibles.Data)
                     .Where(c => _type == MachineType.Airship
                         ? !Accountant.Config.BlockedCompaniesAirships.Contains(c.Key.CastedName)
                         : !Accountant.Config.BlockedCompaniesSubmersibles.Contains(c.Key.CastedName))
                     .Select(c => (GetName(c.Key.Name, c.Key.ServerId), c.Key, c.Value))
                     .OrderByDescending(p => Accountant.Config.GetPriority(p.Item1)))
            {
                var machine = GenerateCompany(name, company, data, ref GlobalCounter);
                if (machine.ObjectsCount > 0)
                    Headers.Add(machine);
            }

            Color       = GlobalCounter.GetColorHeader();
            DisplayTime = GlobalCounter.GetTime();
        }
    }
}
