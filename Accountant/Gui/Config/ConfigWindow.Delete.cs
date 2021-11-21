using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Gui.Helper;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private void DrawDeletePlotCrops()
    {
        PlotInfo? deleteInfo = null;
        foreach (var (plot, data) in _timers.PlotCropTimers.Data)
        {
            var plotName = $"{plot.Name} ({plot.ToName()} @ {Accountant.GameData.GetWorldName(plot.ServerId)})";
            var draw     = ImGui.TreeNodeEx(plotName);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
                deleteInfo = plot;
            ImGuiRaii.HoverTooltip("Hold Control, Shift and right-click to delete.");

            if (!draw)
                continue;

            var deleteIdx = -1;
            foreach (var (plant, idx) in data.Select((p, idx) => (p, idx)).Where(p => p.p.PlantId != 0))
            {
                var bedName   = PlantInfo.GetPlotName(Accountant.GameData.GetPlotSize(plot.Zone, plot.Plot), (ushort)idx);
                var plantName = Accountant.GameData.FindCrop(plant.PlantId).Name;
                ImGui.Selectable($"{idx + 1:D2} - {bedName}: {plantName}");
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyShift)
                    deleteIdx = idx;
                ImGuiRaii.HoverTooltip("Hold Shift and right-click to delete.");
            }

            ImGui.TreePop();
            if (deleteIdx < 0)
                continue;

            data[deleteIdx] = new PlantInfo();
            _timers.PlotCropTimers.Save(plot, data);
            _timers.PlotCropTimers.Invoke();
        }

        if (deleteInfo == null)
            return;

        if (_timers.PlotCropTimers.Remove(deleteInfo.Value))
            _timers.PlotCropTimers.Invoke();
    }

    private void DrawDeletePrivateCrops()
    {
        PlayerInfo? deleteInfo = null;
        foreach (var (player, data) in _timers.PrivateCropTimers.Data)
        {
            var playerName = $"{player.Name} @ {Accountant.GameData.GetWorldName(player.ServerId)}";
            var draw       = ImGui.TreeNodeEx(playerName);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
                deleteInfo = player;
            ImGuiRaii.HoverTooltip("Hold Control, Shift and right-click to delete.");

            if (!draw)
                continue;

            var deleteIdx = -1;
            foreach (var (plant, idx) in data.Select((p, idx) => (p, idx)).Where(p => p.p.PlantId != 0))
            {
                var bedName   = PlantInfo.GetPrivateName((ushort)idx);
                var plantName = Accountant.GameData.FindCrop(plant.PlantId).Name;
                ImGui.Selectable($"{idx + 1:D2} - {bedName}: {plantName}");
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyShift)
                    deleteIdx = idx;
                ImGuiRaii.HoverTooltip("Hold Shift and right-click to delete.");
            }

            ImGui.TreePop();
            if (deleteIdx < 0)
                continue;

            data[deleteIdx] = new PlantInfo();
            _timers.PrivateCropTimers.Save(player, data);
            _timers.PrivateCropTimers.Invoke();
        }

        if (deleteInfo == null)
            return;

        if (_timers.PrivateCropTimers.Remove(deleteInfo.Value))
            _timers.PrivateCropTimers.Invoke();
    }

    private static void DrawDeletePlayerEntries(IEnumerable<PlayerInfo> players, Action<PlayerInfo> remove)
    {
        PlayerInfo? deleteInfo = null;
        using var   indent     = ImGuiRaii.PushIndent();
        foreach (var player in players)
        {
            var playerName = $"{player.Name} @ {Accountant.GameData.GetWorldName(player.ServerId)}";
            ImGui.Selectable(playerName);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
                deleteInfo = player;
            ImGuiRaii.HoverTooltip("Hold Control, Shift and right-click to delete.");
        }

        if (deleteInfo == null)
            return;

        remove(deleteInfo.Value);
    }

    private void DrawDeleteTaskEntries()
        => DrawDeletePlayerEntries(_timers.TaskTimers.Data.Keys, p =>
        {
            if (_timers.TaskTimers.Remove(p))
                _timers.TaskTimers.Invoke();
        });

    private void DrawDeleteRetainerEntries()
        => DrawDeletePlayerEntries(_timers.RetainerTimers.Data.Keys, p =>
        {
            if (_timers.RetainerTimers.Remove(p))
                _timers.RetainerTimers.Invoke();
        });

    private static void DrawDeleteCompanyEntries(IEnumerable<FreeCompanyInfo> companies, Action<FreeCompanyInfo> remove)
    {
        FreeCompanyInfo? deleteInfo = null;
        using var   indent     = ImGuiRaii.PushIndent();
        foreach (var company in companies)
        {
            var companyName = $"{company.Name} <{company.Tag}> @ {Accountant.GameData.GetWorldName(company.ServerId)}";
            ImGui.Selectable(companyName);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift)
                deleteInfo = company;
            ImGuiRaii.HoverTooltip("Hold Control, Shift and right-click to delete.");
        }

        if (deleteInfo == null)
            return;

        remove(deleteInfo.Value);
    }

    private void DrawDeleteAirshipEntries()
        => DrawDeleteCompanyEntries(_timers.AirshipTimers.Data.Keys, c =>
        {
            if (_timers.AirshipTimers.Remove(c))
                _timers.AirshipTimers.Invoke();
        });

    private void DrawDeleteSubmersibleEntries()
        => DrawDeleteCompanyEntries(_timers.SubmersibleTimers.Data.Keys, c =>
        {
            if (_timers.SubmersibleTimers.Remove(c))
                _timers.SubmersibleTimers.Invoke();
        });

    private void DrawDeleteWheelEntries()
        => DrawDeleteCompanyEntries(_timers.WheelTimers.Data.Keys, c =>
        {
            if (_timers.WheelTimers.Remove(c))
                _timers.WheelTimers.Invoke();
        });

    private void DrawDeleteTab()
    {
        if (!ImGui.BeginTabItem("Delete Entries##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        if (!ImGui.BeginChild("##DeleteTab"))
            return;

        raii.Push(ImGui.EndChild);
        using var id = ImGuiRaii.PushId("Deletion");
        if (ImGui.CollapsingHeader("Crop Entries"))
        {
            DrawDeletePlotCrops();
            DrawDeletePrivateCrops();
        }

        if (ImGui.CollapsingHeader("Retainer Entries"))
            DrawDeleteRetainerEntries();

        if (ImGui.CollapsingHeader("Task Entries"))
            DrawDeleteTaskEntries();

        if (ImGui.CollapsingHeader("Airship Entries"))
            DrawDeleteAirshipEntries();

        if (ImGui.CollapsingHeader("Submersible Entries"))
            DrawDeleteSubmersibleEntries();

        if (ImGui.CollapsingHeader("Wheel Entries"))
            DrawDeleteWheelEntries();
    }
}
