using System.Linq;
using Accountant.Classes;
using Accountant.Gui.Helper;
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

        if (ImGui.CollapsingHeader("Blocked Plots"))
            DrawBlockedPlots();
        if (ImGui.CollapsingHeader("Blocked Players"))
            DrawBlockedPlayers();
        if (ImGui.CollapsingHeader("Blocked Free Companies"))
            DrawBlockedCompanies();
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
            _timerWindow.ResetCropCache();
        }

        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newPlotInfo >> 48 != 0)
            if (Accountant.Config.BlockedPlots.Add(_newPlotInfo))
            {
                Accountant.Config.Save();
                _timerWindow.ResetCropCache();
            }

        _.Pop();
        DrawPlotInfoInput(111, ref _newPlotInfo);
        ImGui.NewLine();
    }

    private void DrawBlockedPlayers()
    {
        using var id = ImGuiRaii.PushId("BlockedPlayers");
        if (!ImGui.BeginTable(string.Empty, 3))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTable);
        ImGui.TableSetupColumn(string.Empty,  ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        ImGui.TableSetupColumn("World",       ImGuiTableColumnFlags.WidthFixed, 125 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Player Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableSetupScrollFreeze(0, 1);

        string? change = null;
        foreach (var castedName in Accountant.Config.BlockedPlayers)
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
            Accountant.Config.BlockedPlayers.Remove(change);
            Accountant.Config.Save();
            _timerWindow.ResetCropCache();
            _timerWindow.ResetRetainerCache();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newWorld != 0 && _newBlockedPlayerName.Any())
            if (Accountant.Config.BlockedPlayers.Add(new PlayerInfo(_newBlockedPlayerName, _newWorld).CastedName))
            {
                Accountant.Config.Save();
                _timerWindow.ResetCropCache();
                _timerWindow.ResetRetainerCache();
            }

        _.Pop();
        ImGui.TableNextColumn();
        DrawWorldsCombo(ref _newWorld);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##NewPlayer", "New Player Name...", ref _newBlockedPlayerName, 32);
        ImGui.NewLine();
    }

    private void DrawBlockedCompanies()
    {
        using var id = ImGuiRaii.PushId("BlockedCompanies");
        if (!ImGui.BeginTable(string.Empty, 3))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTable);
        ImGui.TableSetupColumn(string.Empty,        ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        ImGui.TableSetupColumn("World",             ImGuiTableColumnFlags.WidthFixed, 125 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Free Company Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableSetupScrollFreeze(0, 1);

        string? change = null;
        foreach (var castedName in Accountant.Config.BlockedCompanies)
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
            Accountant.Config.BlockedCompanies.Remove(change);
            Accountant.Config.Save();
            _timerWindow.ResetMachineCache();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newWorld != 0 && _newBlockedCompanyName.Any())
            if (Accountant.Config.BlockedCompanies.Add(new FreeCompanyInfo(_newBlockedCompanyName, _newWorld).CastedName))
            {
                Accountant.Config.Save();
                _timerWindow.ResetMachineCache();
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
