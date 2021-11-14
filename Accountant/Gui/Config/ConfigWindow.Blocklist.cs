using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Gui.Helper;
using Accountant.Gui.Timer;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private string _newBlockedPlayerName  = string.Empty;
    private string _newBlockedCompanyName = string.Empty;
    private ushort _newWorld;

    private void DrawBlocklistsTab()
    {
        if (!ImGui.BeginTabItem("Blocklist##AccountantTabs"))
            return;

        if (_newWorld == 0 && Dalamud.ClientState.LocalPlayer != null)
            _newWorld = (ushort)Dalamud.ClientState.LocalPlayer.CurrentWorld.Id;
        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        if (!ImGui.BeginChild("##BlockListTab"))
            return;

        raii.Push(ImGui.EndChild);

        if (ImGui.CollapsingHeader("Blocked Plots"))
            DrawBlockedPlots();
        if (ImGui.CollapsingHeader("Blocked Players (Crops)"))
            DrawBlockedPlayers("Crops", Accountant.Config.BlockedPlayersCrops, typeof(TimerWindow.CropCache));
        if (ImGui.CollapsingHeader("Blocked Players (Retainers)"))
            DrawBlockedPlayers("Retainers", Accountant.Config.BlockedPlayersRetainers, typeof(TimerWindow.RetainerCache));
        if (ImGui.CollapsingHeader("Blocked Players (Tasks)"))
            DrawBlockedPlayers("Tasks", Accountant.Config.BlockedPlayersTasks, typeof(TimerWindow.TaskCache));
        if (ImGui.CollapsingHeader("Blocked Free Companies (Airships)"))
            DrawBlockedCompanies("Airships", Accountant.Config.BlockedCompaniesAirships, typeof(TimerWindow.MachineCache));
        if (ImGui.CollapsingHeader("Blocked Free Companies (Submersibles)"))
            DrawBlockedCompanies("Submersibles", Accountant.Config.BlockedCompaniesSubmersibles, typeof(TimerWindow.MachineCache));
        if (ImGui.CollapsingHeader("Blocked Free Companies (Aetherial Wheels)"))
            DrawBlockedCompanies("Wheels", Accountant.Config.BlockedCompaniesWheels, typeof(TimerWindow.WheelCache));
    }

    private void DrawBlockedPlots()
    {
        using var id = ImGuiRaii.PushId("BlockedPlots");
        if (!ImGui.BeginTable(string.Empty, 5))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTable);

        ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        SetupPlotHeaders();
        ImGui.TableHeadersRow();

        ulong? change = null;
        foreach (var value in Accountant.Config.BlockedPlots)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            using var font = ImGuiRaii.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}##{value}"))
                change = value;
            font.Pop();
            DrawPlotRow(PlotInfo.FromValue(value));
        }

        if (change != null)
        {
            Accountant.Config.BlockedPlots.Remove(change.Value);
            Accountant.Config.Save();
            _timerWindow.ResetCache(typeof(TimerWindow.CropCache));
        }

        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newPlotInfo >> 48 != 0)
            if (Accountant.Config.BlockedPlots.Add(_newPlotInfo))
            {
                Accountant.Config.Save();
                _timerWindow.ResetCache(typeof(TimerWindow.CropCache));
            }

        _.Pop();
        DrawPlotInfoInput(111, ref _newPlotInfo);
        ImGui.NewLine();
    }

    private void DrawBlockedPlayers(string label, ISet<string> list, Type resetType)
    {
        using var id = ImGuiRaii.PushId($"BlockedPlayers{label}");
        if (!ImGui.BeginTable(string.Empty, 3))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTable);
        ImGui.TableSetupColumn(string.Empty,  ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        ImGui.TableSetupColumn("World",       ImGuiTableColumnFlags.WidthFixed, 125 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Player Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableSetupScrollFreeze(0, 1);

        string? change = null;
        foreach (var castedName in list)
        {
            var info = PlayerInfo.FromCastedName(castedName);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            using var font = ImGuiRaii.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}##{castedName}"))
                change = castedName;
            font.Pop();
            ImGui.TableNextColumn();
            ImGui.Text(Accountant.GameData.GetWorldName(info.ServerId));
            ImGui.TableNextColumn();
            ImGui.Text(info.Name);
        }

        if (change != null)
        {
            list.Remove(change);
            Accountant.Config.Save();
            _timerWindow.ResetCache(resetType);
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newWorld != 0 && _newBlockedPlayerName.Any())
            if (list.Add(new PlayerInfo(_newBlockedPlayerName, _newWorld).CastedName))
            {
                Accountant.Config.Save();
                _timerWindow.ResetCache(resetType);
            }

        _.Pop();
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        DrawWorldsCombo(ref _newWorld);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##NewPlayer", "New Player Name...", ref _newBlockedPlayerName, 32);
        ImGui.NewLine();
    }

    private void DrawBlockedCompanies(string label, ISet<string> list, Type resetType)
    {
        using var id = ImGuiRaii.PushId($"BlockedCompanies{label}");
        if (!ImGui.BeginTable(string.Empty, 3))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTable);
        ImGui.TableSetupColumn(string.Empty,        ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        ImGui.TableSetupColumn("World",             ImGuiTableColumnFlags.WidthFixed, 125 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Free Company Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableSetupScrollFreeze(0, 1);

        string? change = null;
        foreach (var castedName in list)
        {
            var info = FreeCompanyInfo.FromCastedName(castedName);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            using var font = ImGuiRaii.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}##{castedName}"))
                change = castedName;
            font.Pop();
            ImGui.TableNextColumn();
            ImGui.Text(Accountant.GameData.GetWorldName(info.ServerId));
            ImGui.TableNextColumn();
            ImGui.Text(info.Name);
        }

        if (change != null)
        {
            list.Remove(change);
            Accountant.Config.Save();
            _timerWindow.ResetCache(resetType);
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newWorld != 0 && _newBlockedCompanyName.Any())
            if (list.Add(new FreeCompanyInfo(_newBlockedCompanyName, _newWorld).CastedName))
            {
                Accountant.Config.Save();
                _timerWindow.ResetCache(resetType);
            }

        _.Pop();
        ImGui.TableNextColumn();
        DrawWorldsCombo(ref _newWorld);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##NewCompany", "New Free Company Name...", ref _newBlockedCompanyName, 32);
        ImGui.NewLine();
    }
}
