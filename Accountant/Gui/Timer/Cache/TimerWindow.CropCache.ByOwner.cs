using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using OtterLoc.Structs;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    private sealed partial class CropCache
    {
        private CacheObject GeneratePlant(PlantInfo plant, string name)
        {
            var ret = new CacheObject()
            {
                Children   = Array.Empty<CacheObject>(),
                Name       = name,
                IconOffset = 0,
            };

            if (plant.Active())
            {
                var (data, plantName)                   = Accountant.GameData.FindCrop(plant.PlantId);
                var (fin, wilt, wither, color, time, _) = plant.GetCropTimes(Now);
                ret.DisplayTime                         = time;
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

        private CacheObject GenerateOwner(PlayerInfo player, IList<PlantInfo> plants)
        {
            var owner = new CacheObject()
            {
                Name     = GetName(player.Name, player.ServerId),
                Children = new CacheObject[plants.Count],
                Color    = ColorId.NeutralText,
            };
            for (ushort i = 0; i < plants.Count; ++i)
            {
                owner.Children[i] = GeneratePlant(plants[i], Manager.CropTimers!.GetPrivateName(i));
                UpdateParent(owner.Children[i].Color, owner.Children[i].DisplayTime, ref owner.Color, ref owner.DisplayTime);
            }

            if (owner.DisplayTime < Now)
                owner.DisplayString = string.Empty;

            UpdateParent(owner.Color, owner.DisplayTime, ref GlobalColor, ref GlobalTime);
            GlobalColor = GlobalColor.TextToHeader();

            return owner;
        }

        private CacheObject GenerateOwner(PlotInfo plot, IList<PlantInfo> plants)
        {
            var owner = new CacheObject()
            {
                Name     = GetName(plot.GetName(), plot.ServerId),
                Children = new CacheObject[plants.Count],
                Color    = ColorId.NeutralText,
            };
            for (ushort i = 0; i < plants.Count; ++i)
            {
                owner.Children[i] = GeneratePlant(plants[i],
                    Manager.CropTimers!.GetPlotName(Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot), i));
                UpdateParent(owner.Children[i].Color, owner.Children[i].DisplayTime, ref owner.Color, ref owner.DisplayTime);
            }

            if (owner.DisplayTime < Now)
                owner.DisplayString = string.Empty;

            UpdateParent(owner.Color, owner.DisplayTime, ref GlobalColor, ref GlobalTime);
            GlobalColor = GlobalColor.TextToHeader();

            return owner;
        }

        private static void UpdateParent(ColorId newColor, DateTime displayTime, ref ColorId oldColor, ref DateTime oldDisplayTime)
        {
            var tmp = oldColor.CombineColor(newColor);
            if (tmp != oldColor)
            {
                oldColor       = newColor;
                oldDisplayTime = displayTime;
            }
            else if (newColor == oldColor && oldDisplayTime > displayTime)
            {
                oldDisplayTime = displayTime;
            }
        }

        private void UpdateByOwner()
        {
            foreach (var (plot, plants) in Manager.CropTimers!.PlotCrops
                         .Where(p => !Accountant.Config.BlockedPlots.Contains(p.Key.Value)))
                Objects.Add(GenerateOwner(plot, plants));

            foreach (var (player, plants) in Manager.CropTimers!.PrivateCrops
                         .Where(p => !Accountant.Config.BlockedPlayers.Contains(p.Key.CastedName)))
                Objects.Add(GenerateOwner(player, plants));
        }
    }
}
