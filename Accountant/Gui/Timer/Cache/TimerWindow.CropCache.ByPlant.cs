using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Structs;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed partial class CropCache
    {
        private readonly Dictionary<uint, (List<CacheObject>, (CropData, string))> _seenItems = new();

        private void GeneratePlantChild(string childName, PlantInfo plant)
        {
            if (!_seenItems.TryGetValue(plant.PlantId, out var list))
            {
                list = (new List<CacheObject>(), Accountant.GameData.FindCrop(plant.PlantId));
                _seenItems.Add(plant.PlantId, list);
            }

            var (fin, wilt, wither, color, time, _) = plant.GetCropTimes(Now);
            var (_, (data, name))                   = _seenItems[plant.PlantId];
            var cache = new CacheObject
            {
                Name          = childName,
                DisplayTime   = UpdateNextChange(time),
                Color         = color,
                DisplayString = time < Now ? string.Empty : null,
                Icon          = Window._icons[data.Item.Icon],
            };
            cache.TooltipCallback = GenerateTooltip(plant, cache, name, fin, wilt, wither);
            list.Item1.Add(cache);
        }

        private static string GetPlantChildName(string name, PlotInfo plot, int idx)
            => $"{PlantInfo.GetPlotName(Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot), (ushort)idx)}, {name}";

        private static string GetPlantChildName(string name, int idx)
            => $"{name} {idx + 1}";

        private SmallHeader GeneratePlantParent(string name, List<CacheObject> children)
        {
            var ret = new SmallHeader
            {
                Name         = $"{name} ({children.Count})###{name}",
                ObjectsBegin = Objects.Count,
                ObjectsCount = children.Count,
                Color        = ColorId.NeutralText,
                DisplayTime  = DateTime.MinValue,
            };
            Objects.AddRange(children);
            foreach (var child in children)
                UpdateParent(child.Color, child.DisplayTime, ref ret.Color, ref ret.DisplayTime);

            return ret;
        }

        private static string NameWithoutCount(string nameWithCount)
        {
            var pos = nameWithCount.IndexOf('(');
            if (pos < 1 || nameWithCount[pos - 1] != ' ')
                return nameWithCount;

            return nameWithCount[..(pos - 1)];
        }

        private void UpdateByCrop()
        {
            _seenItems.Clear();
            foreach (var (plot, plants) in _plotCrops.Data
                         .Where(p => !Accountant.Config.BlockedPlots.Contains(p.Key.Value)))
            {
                var plotName = GetName(plot.Name, plot.ServerId);
                var plotSize = Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot);
                var count    = plants.Length - plotSize.IndoorBeds();
                foreach (var (plant, idx) in plants
                             .Select((p, i) => (p, i))
                             .Where(p => p.p.PlantId != 0))
                {
                    if (!Accountant.Config.IgnoreIndoorPlants || idx < count)
                        GeneratePlantChild(GetPlantChildName(plotName, plot, idx), plant);
                }
            }

            foreach (var (player, plants) in _privateCrops.Data
                         .Where(p => !Accountant.Config.BlockedPlayersCrops.Contains(p.Key.CastedName)))
            {
                var playerName = GetName(player.Name, player.ServerId);
                foreach (var (plant, idx) in plants
                             .Select((p, i) => (p, i))
                             .Where(p => p.Item1.PlantId != 0))
                    GeneratePlantChild(GetPlantChildName(playerName, idx), plant);
            }

            foreach (var (_, (list, (_, name))) in _seenItems)
            {
                Headers.Add(GeneratePlantParent(name, list));
                UpdateParent(Headers.Last().Color.TextToHeader(), Headers.Last().DisplayTime, ref Color, ref DisplayTime);
            }

            if (Accountant.Config.Priorities.Count > 0)
                Headers.Sort((a, b)
                    => Accountant.Config.GetPriority(NameWithoutCount(b.Name))
                        .CompareTo(Accountant.Config.GetPriority(NameWithoutCount(a.Name))));
        }
    }
}
