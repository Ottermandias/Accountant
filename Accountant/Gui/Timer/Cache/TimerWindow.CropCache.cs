using System;
using System.Globalization;
using System.Numerics;
using Accountant.Classes;
using Accountant.Gui.Helper;
using Accountant.Manager;
using Accountant.Timers;
using ImGuiNET;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal sealed partial class CropCache : BaseCache
    {
        private readonly PlotCropTimers    _plotCrops;
        private readonly PrivateCropTimers _privateCrops;

        public CropCache(TimerWindow window, ConfigFlags requiredFlags, PlotCropTimers plotCrops, PrivateCropTimers privateCrops)
            : base("Crops", requiredFlags, window)
        {
            _plotCrops            =  plotCrops;
            _privateCrops         =  privateCrops;
            _plotCrops.Changed    += Resetter;
            _privateCrops.Changed += Resetter;
        }

        protected override void DrawTooltip()
        {
            if (Accountant.Config.ShowCropTooltip)
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("Outdoor crops only refresh every 63 minutes on a ward-specific update timer.\n"
                  + "Any timer may be delayed by up to 63 minutes.\n"
                  + "Fertilizing a plant during these delays will automatically trigger updates.\n"
                  + "They will still grow out/wilt/wither in order, and grown-out plants do not wither anymore.\n"
                  + "Indoors, clearing the house and re-entering should automatically trigger updates.\n"
                  + "You can disable this tooltip in the configuration.");
                ImGui.EndTooltip();
            }
        }

        private static string TimeSpanString2(DateTime target, DateTime now)
        {
            if (target == DateTime.MinValue)
                return "Already";
            if (target == DateTime.UnixEpoch)
                return "Unknown";
            if (target == DateTime.MaxValue)
                return "Never";

            return target < now ? "Already" : TimeSpanString(target - now, 3);
        }

        private static Action GenerateTooltip(PlantInfo plant, CacheObject ret, string plantName, DateTime fin, DateTime wilt, DateTime wither)
        {
            var plantTimeString = plant.PlantTime.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            return () =>
            {
                ImGui.BeginTooltip();
                using var _ = ImGuiRaii.PushColor(ImGuiCol.Button, ret.Color.TextToHeader().Value());
                ImGui.Image(ret.Icon.ImGuiHandle, Vector2.One * ret.Icon.Height / 2);
                ImGui.SameLine();
                ImGui.Button(plantName, Vector2.UnitY * ret.Icon.Height / 2 - Vector2.UnitX);
                ImGui.BeginGroup();
                ImGui.Text("Planted:");
                ImGui.Text("Tended:");
                ImGui.Text("Finished:");
                ImGui.Text("Wilting:");
                ImGui.Text("Withering:");
                if (plant.Position != Vector3.Zero)
                    ImGui.Text("Position:");
                ImGui.EndGroup();
                ImGui.SameLine();
                if (!plant.AccuratePlantTime)
                {
                    ImGui.BeginGroup();
                    ImGui.Text("<");
                    ImGui.NewLine();
                    ImGui.Text(fin != DateTime.MaxValue && fin > DateTime.Now ? "<" : "  ");
                    ImGui.EndGroup();
                }
                else
                {
                    ImGui.Text("  ");
                }

                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.Text(plantTimeString);
                ImGui.Text(plant.LastTending.ToLocalTime().ToString(CultureInfo.CurrentCulture));
                ImGui.Text(TimeSpanString2(fin, DateTime.UtcNow));
                ImGui.Text(fin < wilt ? "Never" : TimeSpanString2(wilt,     DateTime.UtcNow));
                ImGui.Text(fin < wither ? "Never" : TimeSpanString2(wither, DateTime.UtcNow));
                if (plant.Position != Vector3.Zero)
                    ImGui.Text(FormattableString.Invariant($"({plant.Position.X:F1}, {plant.Position.Y:F1}, {plant.Position.Z:F1})"));
                ImGui.EndGroup();
                ImGui.EndTooltip();
            };
        }

        protected override void UpdateInternal()
        {
            if (Accountant.Config.OrderByCrop)
                UpdateByCrop();
            else
                UpdateByOwner();
        }
    }
}
