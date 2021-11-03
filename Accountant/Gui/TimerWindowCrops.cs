using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui;

//public partial class TimerWindow
//{
//    private readonly struct CropInfo
//    {
//        public struct BedInfo
//        {
//            public string BedName;
//            public string CropName;
//            public string FinishTime;
//            public string WiltTime;
//            public string WitherTime;
//            public uint   Color;
//        }
//
//        public static List<BedInfo> AllBeds = new();
//
//        public readonly  PlotCrops?    Plot;
//        public readonly  PrivateCrops? Private;
//        public readonly  string        Name;
//        public readonly  TimeSpan      TimeToWilt;
//        public readonly  TimeSpan      TimeToFinish;
//        public readonly  TimeSpan      TimeToWither;
//        public readonly  uint          Color;
//        private readonly int           FirstBed;
//        private readonly int           NumBeds;
//
//        public IEnumerable<BedInfo> Beds
//            => AllBeds.Skip(FirstBed).Take(NumBeds);
//
//        public CropInfo(DateTime now, dynamic plot)
//        {
//            Plot         = plot as PlotCrops;
//            Private      = plot as PrivateCrops;
//            Name         = (string)plot.GetName();
//            FirstBed     = AllBeds.Count;
//            NumBeds      = 0;
//            TimeToFinish = TimeSpan.MaxValue;
//            TimeToWilt   = TimeSpan.MaxValue;
//            TimeToWither = TimeSpan.MaxValue;
//            Color        = LightGreenText;
//            var nowMinus = now.AddDays(-1);
//            for (ushort i = 0; i < (int)plot.Beds.Length; ++i)
//            {
//                var bed = (PlantInformation)plot.Beds[i];
//                if (bed.PlantId == 0)
//                    continue;
//
//                var finishTime  = bed.FinishTime();
//                var wiltingTime = bed.WiltingTime();
//                var (fin, wilt, die, color) = GetCropTimes(now, finishTime, wiltingTime);
//                AllBeds.Add(new BedInfo()
//                {
//                    BedName    = (string)plot.GetName(i),
//                    CropName   = Crops.Crops.Find(bed.PlantId).Name,
//                    FinishTime = fin,
//                    WiltTime   = wilt,
//                    WitherTime = die,
//                    Color      = color,
//                });
//                ++NumBeds;
//                var diff = finishTime - now;
//                if (diff < TimeToFinish && finishTime != DateTime.MinValue)
//                    TimeToFinish = diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
//                diff = wiltingTime - now;
//                if (diff < TimeToWilt && wiltingTime != DateTime.MinValue)
//                    TimeToWilt = diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
//                diff = wiltingTime - nowMinus;
//                if (diff < TimeToWither && wiltingTime != DateTime.MinValue)
//                    TimeToWither = diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
//                Color = CombineColor(Color, color);
//            }
//        }
//
//        public static string Text(uint color, TimeSpan finish, TimeSpan wilt, TimeSpan wither)
//        {
//            return color switch
//            {
//                LightGreenText => finish == TimeSpan.MaxValue ? string.Empty : TimeSpanString(finish),
//                YellowText     => wilt == TimeSpan.MaxValue ? "Unknown" : TimeSpanString(wilt),
//                GreenText      => wilt == TimeSpan.MaxValue ? string.Empty : TimeSpanString(wilt),
//                _              => TimeSpanString(wither),
//            };
//        }
//    }
//
//    private static (string, string, string, uint) GetCropTimes(DateTime now, DateTime finishTime, DateTime wiltingTime)
//    {
//        var dyingTime = wiltingTime.AddDays(1);
//        if (finishTime == DateTime.MinValue)
//        {
//            if (wiltingTime == DateTime.MinValue)
//                return ("Unknown", "Unknown", "Unknown", YellowText);
//            if (dyingTime < now)
//                return ("Never", "Already", "Already", RedText);
//            if (wiltingTime < now)
//                return ("Unknown", "Already", TimeSpanString(dyingTime - now), PurpleText);
//
//            return ("Unknown", TimeSpanString(wiltingTime - now, 3), TimeSpanString(dyingTime - now, 3), YellowText);
//        }
//
//        if (finishTime < now)
//        {
//            if (finishTime < dyingTime)
//                return ("Already", "Never", "Never", GreenText);
//
//            return ("Never", "Already", "Already", RedText);
//        }
//
//        if (wiltingTime == DateTime.MinValue)
//            return (TimeSpanString(finishTime - now, 3), "Unknown", "Unknown", YellowText);
//        if (dyingTime < now)
//            return ("Never", "Already", "Already", RedText);
//
//        if (wiltingTime < now)
//        {
//            if (dyingTime > finishTime)
//                return (TimeSpanString(finishTime - now, 3), "Already", "Never", LightGreenText);
//
//            return (TimeSpanString(finishTime - now, 3), "Already", TimeSpanString(dyingTime - now, 3), PurpleText);
//        }
//
//        if (wiltingTime > finishTime)
//            return (TimeSpanString(finishTime - now, 3), "Never", "Never", LightGreenText);
//
//        if (dyingTime > finishTime)
//            return (TimeSpanString(finishTime - now, 3), TimeSpanString(wiltingTime - now, 3), "Never", LightGreenText);
//
//        return (TimeSpanString(finishTime - now, 3), TimeSpanString(wiltingTime - now, 3), TimeSpanString(dyingTime - now, 3), YellowText);
//    }
//
//    private readonly List<CropInfo> _cropInfo         = new();
//    private          TimeSpan       _cropTimeToWilt   = TimeSpan.MaxValue;
//    private          TimeSpan       _cropTimeToFinish = TimeSpan.MaxValue;
//    private          TimeSpan       _cropTimeToWither = TimeSpan.MaxValue;
//    private          uint           _cropColor        = LightGreenText;
//
//    private void SortCrops()
//    {
//        _cropInfo.Sort((c1, c2) =>
//        {
//            var x = c1.TimeToWither.CompareTo(c2.TimeToWither);
//            if (x != 0)
//                return x;
//
//            x = c1.TimeToWilt.CompareTo(c2.TimeToWilt);
//            if (x != 0)
//                return x;
//
//            return c1.TimeToFinish.CompareTo(c2.TimeToFinish);
//        });
//    }
//
//    private void GatherCropInfo()
//    {
//        void Update(in CropInfo info)
//        {
//            if (info.TimeToFinish < _cropTimeToFinish)
//                _cropTimeToFinish = info.TimeToFinish;
//            if (info.TimeToWilt < _cropTimeToWilt)
//                _cropTimeToWilt = info.TimeToWilt;
//            if (info.TimeToWither < _cropTimeToWither)
//                _cropTimeToWither = info.TimeToWither;
//            _cropColor = CombineColor(_cropColor, info.Color);
//        }
//
//        CropInfo.AllBeds.Clear();
//        _cropInfo.Clear();
//        _cropTimeToFinish = TimeSpan.MaxValue;
//        _cropTimeToWilt   = TimeSpan.MaxValue;
//        _cropTimeToWither = TimeSpan.MaxValue;
//        _cropColor        = LightGreenText;
//        foreach (var plot in Peon.Timers.Crops.Plots)
//        {
//            _cropInfo.Add(new CropInfo(_now, plot));
//            Update(_cropInfo[^1]);
//        }
//
//        foreach (var plot in Peon.Timers.Crops.PrivateCrops)
//        {
//            _cropInfo.Add(new CropInfo(_now, plot));
//            Update(_cropInfo[^1]);
//        }
//    }
//
//    private bool CropHeader()
//    {
//        ImGui.PushStyleColor(ImGuiCol.Header, TextToHeader(_cropColor));
//        var text = CropInfo.Text(_cropColor, _cropTimeToFinish, _cropTimeToWilt, _cropTimeToWither);
//
//        ImGui.BeginGroup();
//        var collapse = ImGui.CollapsingHeader("Crops");
//        ImGui.SameLine((_widthTotal - _widthShortTime) * ImGuiHelpers.GlobalScale
//          - 2 * ImGui.GetStyle().ItemSpacing.X
//          - ImGui.GetStyle().ScrollbarSize);
//        ImGui.Text(text);
//        ImGui.EndGroup();
//        ImGui.PopStyleColor();
//        return collapse;
//    }
//
//    private void DrawCropTooltip(in CropInfo.BedInfo bed)
//    {
//        if (!ImGui.IsItemHovered())
//            return;
//
//        using var tt = ImGuiRaii.NewTooltip();
//        if (!tt.Begin(() => ImGui.BeginTable("##", 2, ImGuiTableFlags.SizingFixedFit), ImGui.EndTable))
//            return;
//
//        ImGui.TableNextRow();
//        ImGui.TableNextColumn();
//        ImGui.Text("Finished:");
//        ImGui.TableNextColumn();
//        ImGui.Text(bed.FinishTime);
//        ImGui.TableNextRow();
//        ImGui.TableNextColumn();
//        ImGui.Text("Wilting:");
//        ImGui.TableNextColumn();
//        ImGui.Text(bed.WiltTime);
//        ImGui.TableNextRow();
//        ImGui.TableNextColumn();
//        ImGui.Text("Dying:");
//        ImGui.TableNextColumn();
//        ImGui.Text(bed.WitherTime);
//        ImGui.TableNextColumn();
//    }
//
//    private void DrawCrop(in CropInfo crop)
//    {
//        foreach (var bed in crop.Beds)
//        {
//            ImGui.BeginGroup();
//            ImGui.PushStyleColor(ImGuiCol.Text, bed.Color);
//            ImGui.Text(bed.BedName);
//            var offset = ImGui.CalcTextSize(bed.CropName).X + ImGui.GetStyle().ItemInnerSpacing.X;
//            ImGui.SameLine(ImGui.GetContentRegionAvail().X - offset);
//            ImGui.Text(bed.CropName);
//            ImGui.PopStyleColor();
//            ImGui.EndGroup();
//            DrawCropTooltip(bed);
//        }
//    }
//
//    private void DrawCrops()
//    {
//        GatherCropInfo();
//        PlotCrops?    removePlot   = null;
//        PrivateCrops? removePerson = null;
//        if (_drawData && CropHeader())
//        {
//            SortCrops();
//            foreach (var crop in _cropInfo)
//            {
//                ImGui.PushStyleColor(ImGuiCol.Text, crop.Color);
//                var tree = ImGui.TreeNodeEx(crop.Name);
//                if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyCtrl)
//                {
//                    if (crop.Plot != null)
//                        removePlot = crop.Plot;
//                    else
//                        removePerson = crop.Private;
//                }
//
//                if (ImGui.IsItemHovered())
//                    ImGui.SetTooltip("Hold Ctrl and Right-Click to clear.");
//
//                ImGui.SameLine((_widthTotal - _widthShortTime) * ImGuiHelpers.GlobalScale
//                  - 2 * ImGui.GetStyle().ItemSpacing.X
//                  - ImGui.GetStyle().ScrollbarSize);
//                ImGui.Text(CropInfo.Text(crop.Color, crop.TimeToFinish, crop.TimeToWilt, crop.TimeToWither));
//                ImGui.PopStyleColor();
//                if (tree)
//                {
//                    DrawCrop(crop);
//                    ImGui.TreePop();
//                }
//            }
//        }
//
//        if (removePlot != null && Peon.Timers.Crops.Plots.Remove(removePlot)
//         || removePerson != null && Peon.Timers.Crops.PrivateCrops.Remove(removePerson))
//            Peon.Timers.SaveCrops();
//    }
//}
