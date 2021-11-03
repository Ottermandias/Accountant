using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Manager;
using OtterLoc.Structs;

namespace Accountant.Gui;

public partial class TimerWindow
{
    private sealed class MachineCache : ObjectCache
    {
        public MachineCache(TimerWindow window, TimerManager manager)
            : base(window, manager)
        {
            Resubscribe();
        }

        private void Resubscribe()
        {
            if (Manager.MachineTimers != null)
                Manager.MachineTimers.MachineChanged += Resetter;
        }

        private CacheObject GenerateMachine(MachineInfo machine)
            => new()
            {
                Name          = machine.Name,
                Children      = Array.Empty<CacheObject>(),
                DisplayTime   = machine.Arrival,
                Icon          = Window._icons[machine.Type == MachineType.Airship ? Icons.AirshipIcon : Icons.SubmarineIcon],
                IconOffset    = 0.125f,
                DisplayString = GetDisplayInfo(machine.Arrival),
                Color         = ColorId.NeutralText,
            };

        private CacheObject GenerateCompany(FreeCompanyInfo company, IEnumerable<MachineInfo> machines)
        {
            ResetCurrent();
            var newObject = new CacheObject()
            {
                Name     = GetName(company.Name, company.ServerId),
                Children = machines.Where(m => m.Type != MachineType.Unknown).Select(GenerateMachine).ToArray(),
            };
            if (newObject.Children.Length == 0)
                return newObject;

            if (CurrentSentObjects + CurrentCompletedObjects == 4)
            {
                CurrentLimitedObjects   = CurrentAvailableObjects;
                CurrentAvailableObjects = 0;
                for (var i = 0; i < newObject.Children.Length; ++i)
                {
                    if (newObject.Children[i].DisplayString != StringId.Available.Value())
                        continue;

                    newObject.Children[i].Color         = ColorId.DisabledText;
                    newObject.Children[i].DisplayString = "Limited";
                }
            }

            AddCurrent();

            if (CurrentSentObjects == newObject.Children.Length - CurrentLimitedObjects)
            {
                newObject.Color       =  ColorId.TextObjectsAway;
                newObject.DisplayTime =  CurrentActualTimeForFirst;
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

            return newObject;
        }

        private void SetGlobals()
        {
            Header = $"{StringId.Machines.Value()}: {CompletedObjects} | {AvailableObjects} | {SentObjects}";
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
            LimitedObjects = 0;
            foreach (var (company, machines) in Manager.MachineTimers!.Machines)
            {
                var p = GenerateCompany(company, machines);
                if (p.Children.Length == 0)
                    continue;

                Objects.Add(p);
            }

            SetGlobals();
        }
    }
}
