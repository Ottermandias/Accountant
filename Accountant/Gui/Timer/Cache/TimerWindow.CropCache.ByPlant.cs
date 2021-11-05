using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Structs;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    private sealed partial class CropCache
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
                Children      = Array.Empty<CacheObject>(),
                DisplayTime   = time,
                Color         = color,
                DisplayString = time < Now ? string.Empty : null,
                Icon          = Window._icons[data.Item.Icon],
            };
            cache.TooltipCallback = GenerateTooltip(plant, cache, name, fin, wilt, wither);
            list.Item1.Add(cache);
        }

        private string GetPlantChildName(string name, PlotInfo plot, int idx)
            => $"{Manager.CropTimers!.GetPlotName(Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot), (ushort)idx)}, {name}";

        private static string GetPlantChildName(string name, int idx)
            => $"{name} {idx + 1}";

        private CacheObject GeneratePlantParent(string name, List<CacheObject> children)
        {
            var ret = new CacheObject()
            {
                Name     = $"{name} ({children.Count})",
                Children = children.ToArray(),
                Color    = ColorId.NeutralText,
            };
            foreach (var child in ret.Children)
            {
                var newColor = ret.Color.CombineColor(child.Color);
                if (newColor != ret.Color)
                {
                    ret.Color       = newColor;
                    ret.DisplayTime = child.DisplayTime;
                }
                else if (ret.DisplayTime > child.DisplayTime)
                {
                    ret.DisplayTime = child.DisplayTime;
                }
            }

            if (ret.DisplayTime < Now)
                ret.DisplayString = string.Empty;
            return ret;
        }

        private void UpdateByCrop()
        {
            _seenItems.Clear();
            foreach (var (plot, plants) in Manager.CropTimers!.PlotCrops
                         .Where(p => !Accountant.Config.BlockedPlots.Contains(p.Key.Value)))
            {
                var plotName = GetName(plot.GetName(), plot.ServerId);
                foreach (var (plant, idx) in plants.Select((p, i) => (p, i)).Where(p => p.Item1.PlantId != 0))
                    GeneratePlantChild(GetPlantChildName(plotName, plot, idx), plant);
            }

            foreach (var (player, plants) in Manager.CropTimers!.PrivateCrops
                         .Where(p => !Accountant.Config.BlockedPlayers.Contains(p.Key.CastedName)))
            {
                var playerName = GetName(player.Name, player.ServerId);
                foreach (var (plant, idx) in plants.Select((p, i) => (p, i)).Where(p => p.Item1.PlantId != 0))
                    GeneratePlantChild(GetPlantChildName(playerName, idx), plant);
            }

            foreach (var (_, (list, (_, name))) in _seenItems)
            {
                Objects.Add(GeneratePlantParent(name, list));
                var newColor = GlobalColor.CombineColor(Objects.Last().Color.TextToHeader());
                if (newColor != GlobalColor)
                {
                    GlobalColor = newColor;
                    GlobalTime  = Objects.Last().DisplayTime;
                }
                else if (GlobalTime > Objects.Last().DisplayTime)
                {
                    GlobalTime = Objects.Last().DisplayTime;
                }
            }
        }
    }
}
