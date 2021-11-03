using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Manager;
using ImGuiNET;
using OtterLoc.Structs;
using Array = System.Array;

namespace Accountant.Gui;

public partial class TimerWindow
{
    private sealed class CropCache : BaseCache
    {
        public DateTime GlobalTime  = DateTime.MinValue;
        public ColorId  GlobalColor = 0;

        private DateTime CurrentFinishTime = DateTime.UnixEpoch;
        private DateTime CurrentWiltTime   = DateTime.UnixEpoch;
        private DateTime CurrentWitherTime = DateTime.UnixEpoch;
        private ColorId  CurrentColor      = ColorId.NeutralText;

        public CropCache(TimerWindow window, TimerManager manager)
            : base(window, manager)
        {
            Resubscribe();
        }

        private void Resubscribe()
        {
            if (Manager.CropTimers != null)
                Manager.CropTimers.CropChanged += Resetter;
        }

        private void UpdateByCrop()
        { }

        private void ResetCurrent()
        {
            CurrentColor      = ColorId.NeutralText;
            CurrentFinishTime = DateTime.UnixEpoch;
            CurrentWiltTime   = DateTime.UnixEpoch;
            CurrentWitherTime = DateTime.UnixEpoch;
        }

        private void UpdateCurrent(DateTime fin, DateTime wilt, DateTime wither, ColorId color)
        {
            CurrentColor = CurrentColor.CombineColor(color);
            if (fin < CurrentFinishTime)
                CurrentFinishTime = fin;
            if (wilt < CurrentWiltTime)
                CurrentWiltTime = wilt;
            if (wither < CurrentWitherTime)
                CurrentWitherTime = wither;
        }

        private static string Accurate(bool accurate)
            => accurate ? string.Empty : "< ";

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
                var (data, plantName)                          = Accountant.GameData.FindCrop(plant.PlantId);
                var (fin, wilt, wither, color, time, accurate) = plant.GetCropTimes(Now);
                ret.DisplayTime                                = time;
                ret.DisplayString                              = time < Now ? string.Empty : null;
                ret.Icon                                       = Window._icons[data.Item.Icon];
                ret.Color                                      = color;

                void Tooltip()
                {
                    ImGui.BeginTooltip();
                    if (ImGui.BeginTable("", 2))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(plantName);
                        ImGui.TableNextColumn();
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("Planted:");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{Accurate(plant.AccuratePlantTime)}{plant.PlantTime.ToString(CultureInfo.InvariantCulture)}");
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("Tended:");
                        ImGui.TableNextColumn();
                        ImGui.Text(plant.LastTending.ToString(CultureInfo.InvariantCulture));
                        if (plant.Position != Vector3.Zero)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text("Position:");
                            ImGui.TableNextColumn();
                            ImGui.Text(FormattableString.Invariant($"({plant.Position.X:F1}, {plant.Position.Y:F1}, {plant.Position.Z:F1})"));
                        }

                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("Finished:");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{Accurate(accurate)}{TimeSpanString(fin - DateTime.UtcNow, 3)}");
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("Wilting:");
                        ImGui.TableNextColumn();
                        ImGui.Text(TimeSpanString(wilt - DateTime.UtcNow, 3));
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("Withering:");
                        ImGui.TableNextColumn();
                        ImGui.Text(TimeSpanString(wither - DateTime.UtcNow, 3));

                        ImGui.EndTable();
                    }

                    ImGui.EndTooltip();
                }

                ret.TooltipCallback = Tooltip;

                UpdateCurrent(fin, wilt, wither, color);
            }
            else
            {
                ret.Color         = ColorId.NeutralText;
                ret.DisplayString = StringId.Available.Value();
                ret.Icon          = Window._icons[Icons.PottingSoilIcon];
            }

            return ret;
        }

        private void SetDisplay(ref CacheObject owner)
        {
            switch (owner.Color)
            {
                case ColorId.NeutralText:
                case ColorId.TextCropWithered:
                    owner.DisplayString = string.Empty;
                    break;
                case ColorId.TextCropGrown:
                    if (CurrentWiltTime < Now)
                        owner.DisplayString = string.Empty;
                    else
                        owner.DisplayTime = CurrentWiltTime;
                    break;
                case ColorId.TextCropWilted:
                    owner.DisplayTime = CurrentWitherTime;
                    break;
                case ColorId.TextCropGrowing:
                    owner.DisplayTime = CurrentWiltTime;
                    break;
                case ColorId.TextCropGuaranteed:
                    owner.DisplayTime = CurrentFinishTime;
                    break;
            }

            if (owner.DisplayTime < Now)
                owner.DisplayString = string.Empty;

            var newColor = GlobalColor.CombineColor(owner.Color).TextToHeader();
            if (newColor != GlobalColor)
            {
                GlobalColor = newColor;
                GlobalTime  = owner.DisplayTime;
            }
            else if (GlobalTime > owner.DisplayTime)
            {
                GlobalTime = owner.DisplayTime;
            }
        }

        private CacheObject GenerateOwner(PlayerInfo player, IList<PlantInfo> plants)
        {
            ResetCurrent();
            var owner = new CacheObject()
            {
                Name     = GetName(player.Name, player.ServerId),
                Children = new CacheObject[plants.Count],
            };
            for (ushort i = 0; i < plants.Count; ++i)
                owner.Children[i] = GeneratePlant(plants[i], Manager.CropTimers!.GetPrivateName(i));

            owner.Color = CurrentColor;
            SetDisplay(ref owner);
            

            return owner;
        }

        private CacheObject GenerateOwner(PlotInfo plot, IList<PlantInfo> plants)
        {
            ResetCurrent();
            var owner = new CacheObject()
            {
                Name     = GetName(plot.ToString(), plot.ServerId),
                Children = new CacheObject[plants.Count],
            };
            for (ushort i = 0; i < plants.Count; ++i)
            {
                owner.Children[i] = GeneratePlant(plants[i],
                    Manager.CropTimers!.GetPlotName(Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot), i));
            }

            owner.Color = CurrentColor;
            SetDisplay(ref owner);

            return owner;
        }

        private void UpdateByOwner()
        {
            foreach (var (plot, plants) in Manager.CropTimers!.PlotCrops)
                Objects.Add(GenerateOwner(plot, plants));

            foreach (var (player, plants) in Manager.CropTimers!.PrivateCrops)
                Objects.Add(GenerateOwner(player, plants));
        }

        protected override void UpdateInternal()
        {
            GlobalColor = ColorId.HeaderCropGuaranteed;
            if (Accountant.Config.OrderByCrop)
                UpdateByCrop();
            else
                UpdateByOwner();
        }
    }
}
