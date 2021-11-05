using System;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private static bool DrawWorldsCombo(ref ushort serverId)
    {
        var preview = serverId == 0 ? string.Empty : Accountant.GameData.GetWorldName(serverId);
        if (!ImGui.BeginCombo(string.Empty, preview, ImGuiComboFlags.NoArrowButton))
            return false;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndCombo);
        foreach (var (name, idx) in Accountant.GameData.Worlds())
        {
            if (!ImGui.Selectable(name, idx == serverId) || idx == serverId)
                continue;

            serverId = (ushort)idx;
            return true;
        }

        return false;
    }

    private static void DrawWorldsCombo(PlotInfo current, ref ulong plotInfo)
    {
        using var _ = ImGuiRaii.PushId("Worlds");
        ImGui.SetNextItemWidth(-1);
        var id = current.ServerId;

        if (DrawWorldsCombo(ref id))
            plotInfo = new PlotInfo(current.Zone, current.Ward, current.Plot, id).Value;
    }

    private static void DrawZonesCombo(PlotInfo current, ref ulong plotInfo)
    {
        using var _ = ImGuiRaii.PushId("Zones");
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo(string.Empty, current.Zone == 0 ? string.Empty : current.Zone.ToName(), ImGuiComboFlags.NoArrowButton))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndCombo);
        foreach (var zone in Enum.GetValues<InternalHousingZone>().Skip(1))
        {
            if (ImGui.Selectable(zone.ToName(), zone == current.Zone))
                plotInfo = new PlotInfo(zone, current.Ward, current.Plot, current.ServerId).Value;
        }
    }

    private static void DrawWardCombo(PlotInfo current, ref ulong plotInfo)
    {
        using var _ = ImGuiRaii.PushId("Wards");
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo(string.Empty, current.Ward.ToString("D2"), ImGuiComboFlags.NoArrowButton))
            return;

        using var end = ImGuiRaii.DeferredEnd(ImGui.EndCombo);
        for (ushort i = 1; i <= Accountant.GameData.GetNumWards(); ++i)
        {
            if (ImGui.Selectable(i.ToString("D2"), i == current.Ward))
                plotInfo = new PlotInfo(current.Zone, i, current.Plot, current.ServerId).Value;
        }
    }

    private static void DrawPlotCombo(PlotInfo current, ref ulong plotInfo)
    {
        using var _ = ImGuiRaii.PushId("Plots");
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo(string.Empty, current.Plot.ToString("D2"), ImGuiComboFlags.NoArrowButton))
            return;

        using var end      = ImGuiRaii.DeferredEnd(ImGui.EndCombo);
        var       numPlots = current.Zone == 0 ? Accountant.GameData.GetNumPlots() : Accountant.GameData.GetNumPlots(current.Zone);
        for (ushort i = 1; i <= numPlots; ++i)
        {
            if (ImGui.Selectable(i.ToString("D2"), i == current.Plot))
                plotInfo = new PlotInfo(current.Zone, current.Ward, i, current.ServerId).Value;
        }
    }

    private static void DrawPlotInfoInput(int id, ref ulong plotInfo)
    {
        using var _ = ImGuiRaii.PushId(id);

        var newPlot = PlotInfo.FromValue(plotInfo);
        if (newPlot.ServerId == 0 && Dalamud.ClientState.LocalPlayer != null)
        {
            newPlot  = new PlotInfo(newPlot.Zone, newPlot.Ward, newPlot.Plot, (ushort)Dalamud.ClientState.LocalPlayer.CurrentWorld.Id);
            plotInfo = newPlot.Value;
        }

        ImGui.TableNextColumn();
        DrawWorldsCombo(newPlot, ref plotInfo);
        ImGui.TableNextColumn();
        DrawZonesCombo(newPlot, ref plotInfo);
        ImGui.TableNextColumn();
        DrawWardCombo(newPlot, ref plotInfo);
        ImGui.TableNextColumn();
        DrawPlotCombo(newPlot, ref plotInfo);
    }

    private static void DrawPlotRow(PlotInfo plot)
    {
        ImGui.TableNextColumn();
        ImGui.Text(Accountant.GameData.GetWorldName(plot.ServerId));
        ImGui.TableNextColumn();
        ImGui.Text(plot.Zone.ToName());
        ImGui.TableNextColumn();
        ImGui.Text(plot.Ward.ToString("D2"));
        ImGui.TableNextColumn();
        ImGui.Text(plot.Plot.ToString("D2"));
    }

    private static void SetupPlotHeaders()
    {
        ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed, 125 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Zone",  ImGuiTableColumnFlags.WidthFixed, 125 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Ward",  ImGuiTableColumnFlags.WidthFixed, 35 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Plot",  ImGuiTableColumnFlags.WidthFixed, 35 * ImGuiHelpers.GlobalScale);
    }
}
