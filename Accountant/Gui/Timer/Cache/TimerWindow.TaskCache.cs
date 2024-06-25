using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Timer.Cache;
using Accountant.Timers;
using ImGuiNET;
using OtterLoc.Structs;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed class TaskCache : BaseCache
    {
        private readonly TaskTimers _tasks;

        public TaskCache(TimerWindow window, ConfigFlags required, TaskTimers tasks)
            : base("Tasks", required, window)
        {
            _tasks         =  tasks;
            _tasks.Changed += Resetter;
        }

        private CacheObject LeveObject(string player, int allowances)
            => new()
            {
                Color =
                    allowances > Leve.AllowanceError           ? ColorId.TextLeveCap :
                    allowances > Accountant.Config.LeveWarning ? ColorId.TextLeveWarning : ColorId.NeutralText,
                DisplayString = allowances.ToString(),
                Name          = player,
                Icon          = Icons.LeveQuestIcon,
                IconOffset    = 0.125f,
            };

        private static Action GenerateTooltip(Squadron info)
        {
            var missionName  = info.MissionName() ?? "Squadron Mission";
            var trainingName = info.TrainingName() ?? "Squadron Training";
            return () =>
            {
                ImGui.BeginTooltip();
                var now = DateTime.UtcNow;
                ImGui.BeginGroup();
                ImGui.Text("New Recruits");
                ImGui.Text(missionName);
                ImGui.Text(trainingName);
                ImGui.EndGroup();
                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text(info.NewRecruits ? StringId.Available.Value() : "None");
                if (info.MissionEnd == DateTime.MinValue)
                    ImGui.Text(StringId.Available.Value());
                else if (info.MissionEnd < now)
                    ImGui.Text(StringId.Completed.Value());
                else
                    ImGui.Text(TimeSpanString(info.MissionEnd - now));
                if (info.TrainingEnd == DateTime.MinValue)
                    ImGui.Text(StringId.Available.Value());
                else if (info.TrainingEnd < now)
                    ImGui.Text(StringId.Completed.Value());
                else
                    ImGui.Text(TimeSpanString(info.TrainingEnd - now));
                ImGui.EndGroup();
                ImGui.EndTooltip();
            };
        }

        private static unsafe Action GenerateTooltip(JumboCactpot jumbo)
        {
            var sb = new StringBuilder(Classes.JumboCactpot.MaxTickets * 5);
            for (var i = 0; i < jumbo.Count(); ++i)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(jumbo.Tickets[i].ToString("D4"));
            }

            var ticketString = sb.ToString();
            return () =>
            {
                if (ticketString.Any())
                    ImGui.SetTooltip(ticketString);
            };
        }

        private CacheObject SquadronObject(string player, Squadron info)
        {
            var ret = new CacheObject
            {
                Name        = player,
                IconOffset  = 0,
                Icon        = Icons.SquadronIcon,
                DisplayTime = UpdateNextChange(info.MissionEnd),
            };

            if (info.MissionId == 0)
            {
                ret.DisplayString = StringId.Available.Value();
                ret.Color         = ColorId.NeutralText;
            }
            else if (info.MissionEnd < Now)
            {
                ret.DisplayString = StringId.Completed.Value();
                ret.Color         = ColorId.TextObjectsHome;
            }
            else
            {
                ret.DisplayString = null;
                ret.Color         = ColorId.TextObjectsAway;
            }

            UpdateNextChange(info.TrainingEnd);
            ret.TooltipCallback = GenerateTooltip(info);

            return ret;
        }

        private CacheObject MapObject(string player, DateTime map)
        {
            var ret = new CacheObject
            {
                Name        = player,
                Icon        = Icons.MapIcon,
                DisplayTime = UpdateNextChange(map),
            };
            if (map == DateTime.MinValue || map < Now)
            {
                ret.DisplayString = StringId.Available.Value();
                ret.Color         = ColorId.NeutralText;
            }
            else
            {
                ret.DisplayString = null;
                ret.Color         = ColorId.TextObjectsAway;
            }

            return ret;
        }

        private CacheObject MiniCactpotObject(string player, MiniCactpot mini)
        {
            var nextReset = mini.NextReset();
            var ret = new CacheObject
            {
                Name        = player,
                Icon        = Icons.MiniCactpotIcon,
                DisplayTime = UpdateNextChange(nextReset),
            };
            if (nextReset < Now || mini.Tickets == 0)
            {
                ret.DisplayString = $"0/{Classes.MiniCactpot.MaxTickets}";
                ret.Color         = ColorId.TextObjectsHome;
            }
            else if (mini.Tickets == Classes.MiniCactpot.MaxTickets)
            {
                ret.DisplayString = null;
                ret.Color         = ColorId.TextObjectsAway;
            }
            else
            {
                ret.DisplayString = $"{mini.Tickets}/{Classes.MiniCactpot.MaxTickets}";
                ret.Color         = ColorId.TextObjectsMixed;
            }

            return ret;
        }

        private CacheObject JumboCactpotObject(string player, ushort worldId, JumboCactpot jumbo)
        {
            var nextReset   = jumbo.NextReset(worldId);
            var doubleReset = nextReset.AddDays(7);
            var ret = new CacheObject
            {
                Name            = player,
                Icon            = Icons.JumboCactpotIcon,
                DisplayTime     = UpdateNextChange(nextReset),
                TooltipCallback = GenerateTooltip(jumbo),
            };
            if (doubleReset < Now || jumbo.IsEmpty())
            {
                ret.DisplayString = $"0/{Classes.JumboCactpot.MaxTickets}";
                ret.Color         = ColorId.TextObjectsHome;
            }
            else if (nextReset < Now && !jumbo.IsEmpty())
            {
                ret.DisplayString = "Redeemable";
                ret.Color         = ColorId.TextObjectsHome;
            }
            else if (jumbo.IsFull())
            {
                ret.DisplayString = null;
                ret.Color         = ColorId.TextObjectsAway;
            }
            else
            {
                ret.DisplayString = $"{jumbo.Count()}/{Classes.JumboCactpot.MaxTickets}";
                ret.Color         = ColorId.TextObjectsMixed;
            }

            return ret;
        }

        private CacheObject DeliveryObject(string player, Delivery delivery)
        {
            var nextReset = Delivery.NextReset(DateTime.UtcNow);
            var ret = new CacheObject
            {
                Name        = player,
                Icon        = Icons.CustomDeliveryIcon,
                DisplayTime = UpdateNextChange(nextReset),
            };
            var allowances = delivery.CurrentAllowances(DateTime.UtcNow);
            (ret.DisplayString, ret.Color) = allowances switch
            {
                0                     => ($"0/{Delivery.AllowanceCap}", ColorId.TextObjectsAway),
                Delivery.AllowanceCap => ($"{Delivery.AllowanceCap}/{Delivery.AllowanceCap}", ColorId.TextObjectsHome),
                _                     => ($"{allowances}/{Delivery.AllowanceCap}", ColorId.TextObjectsMixed),
            };
            return ret;
        }

        private CacheObject TribeObject(string player, Tribe tribe)
        {
            var nextReset = Tribe.NextReset(DateTime.UtcNow);
            var ret = new CacheObject
            {
                Name        = player,
                Icon        = Icons.TribeIcon,
                DisplayTime = UpdateNextChange(nextReset),
            };
            var allowances = tribe.CurrentAllowances(DateTime.UtcNow);
            (ret.DisplayString, ret.Color) = allowances switch
            {
                0 => ($"0/{Tribe.AllowanceCap}", ColorId.TextObjectsAway),
                Tribe.AllowanceCap => ($"{Tribe.AllowanceCap}/{Tribe.AllowanceCap}", ColorId.TextObjectsHome),
                _ when allowances <= Accountant.Config.TribesFinished => ($"{allowances}/{Tribe.AllowanceCap}", ColorId.TextObjectsAway),
                _ => ($"{allowances}/{Tribe.AllowanceCap}", ColorId.TextObjectsMixed),
            };
            return ret;
        }

        private SmallHeader Leves(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MinValue,
                Color        = ColorId.NeutralText,
            };
            var leveSum = 0;
            foreach (var (name, leves) in data.Select(p => (p.Item1, p.Item3.Leves.CurrentAllowances(Now))))
            {
                Objects.Add(LeveObject(name, leves));
                if (leves < Leve.AllowanceCap)
                    ret.DisplayTime = UpdateNextChange(Leve.Round(Now).AddHours(12));
                ret.Color =  ret.Color.Combine(Objects.Last().Color);
                leveSum   += leves;
            }

            ret.Name = $"Leve Allowances ({leveSum})###LeveAllowances";
            return ret;
        }

        private SmallHeader Squadrons(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Squadrons",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = ColorId.NeutralText,
            };
            foreach (var (name, _, task) in data)
            {
                Objects.Add(SquadronObject(name, task.Squadron));
                if (Objects.Last().DisplayTime > Now && Objects.Last().DisplayTime < ret.DisplayTime)
                    ret.DisplayTime = Objects.Last().DisplayTime;
                ret.Color = Objects.Last().Color switch
                {
                    ColorId.TextObjectsAway => ret.Color == ColorId.TextObjectsHome ? ColorId.TextObjectsMixed : ColorId.TextObjectsAway,
                    ColorId.TextObjectsHome => ret.Color == ColorId.TextObjectsAway ? ColorId.TextObjectsMixed : ColorId.TextObjectsHome,
                    _                       => ret.Color,
                };
            }

            return ret;
        }

        private SmallHeader Maps(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Map Allowance",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = ColorId.NeutralText,
            };
            foreach (var (name, _, task) in data)
            {
                Objects.Add(MapObject(name, task.Map));
                if (Objects.Last().DisplayTime > Now && Objects.Last().DisplayTime < ret.DisplayTime)
                    ret.DisplayTime = Objects.Last().DisplayTime;
                ret.Color = Objects.Last().Color switch
                {
                    ColorId.TextObjectsAway => ret.Color == ColorId.TextObjectsHome ? ColorId.TextObjectsMixed : ColorId.TextObjectsAway,
                    _                       => ret.Color == ColorId.NeutralText ? ColorId.TextObjectsHome : ret.Color,
                };
            }

            return ret;
        }

        private SmallHeader MiniCactpot(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Mini Cactpot",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = data.Count > 0 ? ColorId.NeutralText : ColorId.TextObjectsHome,
            };
            foreach (var (name, _, task) in data)
            {
                Objects.Add(MiniCactpotObject(name, task.MiniCactpot));
                if (Objects.Last().DisplayTime > Now && Objects.Last().DisplayTime < ret.DisplayTime)
                    ret.DisplayTime = Objects.Last().DisplayTime;
                ret.Color = Objects.Last().Color switch
                {
                    ColorId.TextObjectsAway => ret.Color is ColorId.TextObjectsAway or ColorId.NeutralText
                        ? ColorId.TextObjectsAway
                        : ColorId.TextObjectsMixed,
                    ColorId.TextObjectsHome => ret.Color is ColorId.TextObjectsHome or ColorId.NeutralText
                        ? ColorId.TextObjectsHome
                        : ColorId.TextObjectsMixed,
                    _ => ColorId.TextObjectsMixed,
                };
            }

            return ret;
        }

        private SmallHeader JumboCactpot(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Jumbo Cactpot",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = ColorId.TextObjectsAway,
            };
            foreach (var (name, serverId, task) in data)
            {
                Objects.Add(JumboCactpotObject(name, serverId, task.JumboCactpot));
                if (Objects.Last().DisplayTime > Now && Objects.Last().DisplayTime < ret.DisplayTime)
                    ret.DisplayTime = Objects.Last().DisplayTime;
                ret.Color = Objects.Last().Color switch
                {
                    ColorId.TextObjectsAway => ret.Color == ColorId.TextObjectsAway ? ColorId.TextObjectsAway : ColorId.TextObjectsMixed,
                    ColorId.TextObjectsHome => ret.Color == ColorId.TextObjectsHome ? ColorId.TextObjectsHome : ColorId.TextObjectsMixed,
                    _                       => ColorId.TextObjectsMixed,
                };
            }

            return ret;
        }

        private SmallHeader Deliveries(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Custom Deliveries",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = data.Count > 0 ? ColorId.NeutralText : ColorId.TextObjectsHome,
            };
            foreach (var (name, serverId, task) in data)
            {
                Objects.Add(DeliveryObject(name, task.Delivery));
                if (Objects.Last().DisplayTime > Now && Objects.Last().DisplayTime < ret.DisplayTime)
                    ret.DisplayTime = Objects.Last().DisplayTime;
                ret.Color = Objects.Last().Color switch
                {
                    ColorId.TextObjectsAway => ret.Color is ColorId.TextObjectsAway or ColorId.NeutralText ? ColorId.TextObjectsAway : ColorId.TextObjectsMixed,
                    ColorId.TextObjectsHome => ret.Color is ColorId.TextObjectsHome or ColorId.NeutralText ? ColorId.TextObjectsHome : ColorId.TextObjectsMixed,
                    _                       => ColorId.TextObjectsMixed,
                };
            }

            return ret;
        }

        private SmallHeader Tribes(IReadOnlyCollection<(string, ushort, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Tribal Quests",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = data.Count > 0 ? ColorId.NeutralText : ColorId.TextObjectsHome,
            };
            foreach (var (name, serverId, task) in data)
            {
                Objects.Add(TribeObject(name, task.Tribe));
                if (Objects.Last().DisplayTime > Now && Objects.Last().DisplayTime < ret.DisplayTime)
                    ret.DisplayTime = Objects.Last().DisplayTime;
                ret.Color = Objects.Last().Color switch
                {
                    ColorId.TextObjectsAway => ret.Color is ColorId.TextObjectsAway or ColorId.NeutralText ? ColorId.TextObjectsAway : ColorId.TextObjectsMixed,
                    ColorId.TextObjectsHome => ret.Color is ColorId.TextObjectsHome or ColorId.NeutralText ? ColorId.TextObjectsHome : ColorId.TextObjectsMixed,
                    _                       => ColorId.TextObjectsMixed,
                };
            }

            return ret;
        }

        protected override void UpdateInternal()
        {
            if (!Accountant.Config.Flags.Check(ConfigFlags.Enabled)
             || !Accountant.Config.Flags.Any(ConfigFlags.LeveAllowances
                  | ConfigFlags.Squadron
                  | ConfigFlags.MapAllowance
                  | ConfigFlags.MiniCactpot
                  | ConfigFlags.JumboCactpot
                  | ConfigFlags.CustomDelivery
                  | ConfigFlags.Tribes))
                return;

            var data = _tasks.Data
                .Where(p => !Accountant.Config.BlockedPlayersTasks.Contains(p.Key.CastedName))
                .Select(p => (GetName(p.Key.Name, p.Key.ServerId), p.Key.ServerId, p.Value))
                .OrderByDescending(p => Accountant.Config.GetPriority(p.Item1))
                .ToArray();
            if (Accountant.Config.Flags.Check(ConfigFlags.LeveAllowances))
                Headers.Add(Leves(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.Squadron))
                Headers.Add(Squadrons(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.MapAllowance))
                Headers.Add(Maps(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.MiniCactpot))
                Headers.Add(MiniCactpot(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.JumboCactpot))
            {
                _tasks.CheckJumboCactpotReset(Now);
                Headers.Add(JumboCactpot(data));
            }

            if (Accountant.Config.Flags.Check(ConfigFlags.CustomDelivery))
                Headers.Add(Deliveries(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.Tribes))
                Headers.Add(Tribes(data));

            foreach (var header in Headers)
                Color = Color.Combine(header.Color.TextToHeader());

            if (Accountant.Config.Priorities.Count > 0)
                Headers.Sort((a, b)
                    => Accountant.Config.GetPriority(b.Name).CompareTo(Accountant.Config.GetPriority(a.Name)));
        }
    }
}
