﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Accountant.Manager;
using Accountant.Timers;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Data.Files;
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
                Icon          = Window._icons[Icons.LeveQuestIcon],
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

        private CacheObject SquadronObject(string player, Squadron info)
        {
            var ret = new CacheObject
            {
                Name        = player,
                IconOffset  = 0,
                Icon        = Window._icons[Icons.SquadronIcon],
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
                Icon        = Window._icons[Icons.MapIcon],
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


        private SmallHeader Leves(IReadOnlyCollection<(string, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MinValue,
                Color        = ColorId.NeutralText,
            };
            var leveSum = 0;
            foreach (var (name, leves) in data.Select(p => (p.Item1, p.Item2.Leves.CurrentAllowances(Now))))
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

        private SmallHeader Squadrons(IReadOnlyCollection<(string, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Squadrons",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = ColorId.NeutralText,
            };
            foreach (var (name, task) in data)
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

        private SmallHeader Maps(IReadOnlyCollection<(string, TaskInfo)> data)
        {
            var ret = new SmallHeader
            {
                Name         = "Map Allowance",
                ObjectsBegin = Objects.Count,
                ObjectsCount = data.Count,
                DisplayTime  = DateTime.MaxValue,
                Color        = ColorId.NeutralText,
            };
            foreach (var (name, task) in data)
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

        protected override void UpdateInternal()
        {
            if (!Accountant.Config.Flags.Check(ConfigFlags.Enabled)
             || !Accountant.Config.Flags.Any(ConfigFlags.LeveAllowances | ConfigFlags.Squadron | ConfigFlags.MapAllowance))
                return;

            var data = _tasks.Data
                .Where(p => !Accountant.Config.BlockedPlayersTasks.Contains(p.Key.CastedName))
                .Select(p => (GetName(p.Key.Name, p.Key.ServerId), p.Value))
                .OrderByDescending(p => Accountant.Config.GetPriority(p.Item1))
                .ToArray();
            if (Accountant.Config.Flags.Check(ConfigFlags.LeveAllowances))
                Headers.Add(Leves(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.Squadron))
                Headers.Add(Squadrons(data));
            if (Accountant.Config.Flags.Check(ConfigFlags.MapAllowance))
                Headers.Add(Maps(data));

            if (Accountant.Config.Priorities.Count > 0)
                Headers.Sort((a, b)
                    => Accountant.Config.GetPriority(b.Name).CompareTo(Accountant.Config.GetPriority(a.Name)));
        }
    }
}
