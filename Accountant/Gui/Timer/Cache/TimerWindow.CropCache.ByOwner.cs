using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using OtterLoc.Structs;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed partial class CropCache
    {
        private CacheObject GeneratePlant(PlantInfo plant, string name)
        {
            var ret = new CacheObject
            {
                Name       = name,
                IconOffset = 0,
            };

            if (plant.Active())
            {
                var (data, plantName)                   = Accountant.GameData.FindCrop(plant.PlantId);
                var (fin, wilt, wither, color, time, _) = plant.GetCropTimes(Now);
                ret.DisplayTime                         = UpdateNextChange(time);
                ret.DisplayString                       = time < Now ? string.Empty : null;
                ret.Icon                                = Window._icons[data.Item.Icon];
                ret.Color                               = color;
                ret.TooltipCallback                     = GenerateTooltip(plant, ret, plantName, fin, wilt, wither);
            }
            else
            {
                ret.Color         = ColorId.NeutralText;
                ret.DisplayString = StringId.Available.Value();
                ret.Icon          = Window._icons[Icons.PottingSoilIcon];
            }

            return ret;
        }

        private SmallHeader GenerateOwner(PlayerInfo player, IList<PlantInfo> plants)
        {
            var owner = new SmallHeader
            {
                Name         = GetName(player.Name, player.ServerId),
                ObjectsBegin = Objects.Count,
                ObjectsCount = plants.Count,
                Color        = ColorId.NeutralText,
            };

            for (ushort i = 0; i < plants.Count; ++i)
            {
                if (!Accountant.Config.BlockedCrops.Contains(plants[i].PlantId))
                {
                    Objects.Add(GeneratePlant(plants[i], PlantInfo.GetPrivateName(i)));
                    UpdateParent(Objects.Last().Color, Objects.Last().DisplayTime, ref owner.Color, ref owner.DisplayTime);
                }
            }

            UpdateParent(owner.Color.TextToHeader(), owner.DisplayTime, ref Color, ref DisplayTime);
            return owner;
        }

        private SmallHeader GenerateOwner(PlotInfo plot, IList<PlantInfo> plants)
        {
            var owner = new SmallHeader
            {
                Name         = GetName(plot.Name, plot.ServerId),
                ObjectsBegin = Objects.Count,
                Color        = ColorId.NeutralText,
            };

            var plotSize = Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot);
            var count    = plants.Count;
            if (Accountant.Config.IgnoreIndoorPlants)
                count -= plotSize.IndoorBeds();
            var objects = 0;
            for (ushort i = 0; i < count; ++i)
            {
                if (!Accountant.Config.BlockedCrops.Contains(plants[i].PlantId))
                {
                    ++objects;
                    Objects.Add(GeneratePlant(plants[i], PlantInfo.GetPlotName(plotSize, i)));
                    UpdateParent(Objects.Last().Color, Objects.Last().DisplayTime, ref owner.Color, ref owner.DisplayTime);
                }
            }

            owner.ObjectsCount = objects;

            UpdateParent(owner.Color.TextToHeader(), owner.DisplayTime, ref Color, ref DisplayTime);

            return owner;
        }

        private static void UpdateParent(ColorId newColor, DateTime displayTime, ref ColorId oldColor, ref DateTime oldDisplayTime)
        {
            var tmp = oldColor.Combine(newColor);
            if (tmp != oldColor)
            {
                oldColor       = newColor;
            }
            if (displayTime != DateTime.MinValue)
            {
                if (oldDisplayTime == DateTime.MinValue || oldDisplayTime > displayTime)
                    oldDisplayTime = displayTime;
            }
        }

        private void UpdateByOwner()
        {
            foreach (var (plot, plants) in _plotCrops.Data
                         .Where(p => !Accountant.Config.BlockedPlots.Contains(p.Key.Value)))
                Headers.Add(GenerateOwner(plot, plants));

            foreach (var (player, plants) in _privateCrops.Data
                         .Where(p => !Accountant.Config.BlockedPlayersCrops.Contains(p.Key.CastedName)))
                Headers.Add(GenerateOwner(player, plants));

            if (Accountant.Config.Priorities.Count > 0)
                Headers.Sort((a, b) => Accountant.Config.GetPriority(b.Name).CompareTo(Accountant.Config.GetPriority(a.Name)));
        }
    }
}
