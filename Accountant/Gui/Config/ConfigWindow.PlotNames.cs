using System.Linq;
using System.Numerics;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private ulong  _newPlotInfo = new PlotInfo(InternalHousingZone.Mist, 1, 1, 0).Value;
    private string _newPlotName = string.Empty;

    private void DrawPlotNamesTab()
    {
        if (!ImGui.BeginTabItem("Plot Names##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        using var ids = ImGuiRaii.PushId("PlotNames");
        if (!ImGui.BeginTable("", 6, ImGuiTableFlags.None, -Vector2.One))
            return;

        raii.Push(ImGui.EndTable);

        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        SetupPlotHeaders();
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableSetupScrollFreeze(0, 1);

        ulong?  change  = null;
        string? newName = null;
        foreach (var (value, name) in Accountant.Config.PlotNames)
        {
            var tmp = name;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            using var font = ImGuiRaii.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}##{value}"))
                change = value;
            font.Pop();
            DrawPlotRow(PlotInfo.FromValue(value));
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputText($"##{value}", ref tmp, 32, ImGuiInputTextFlags.EnterReturnsTrue) && tmp != name)
            {
                change  = value;
                newName = tmp;
            }
        }

        var newPlot = PlotInfo.FromValue(_newPlotInfo);
        if (newPlot.ServerId == 0 && Dalamud.ClientState.LocalPlayer != null)
        {
            newPlot      = new PlotInfo(newPlot.Zone, newPlot.Ward, newPlot.Plot, (ushort)Dalamud.ClientState.LocalPlayer.CurrentWorld.Id);
            _newPlotInfo = newPlot.Value;
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && newPlot.ServerId != 0)
        {
            change  = _newPlotInfo;
            newName = _newPlotName;
        }

        _.Pop();

        DrawPlotInfoInput(111, ref _newPlotInfo);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##name", "New Plot Name...", ref _newPlotName, 32);

        if (change != null)
        {
            if (newName != null && newName.Any())
                Accountant.Config.PlotNames[change.Value] = newName;
            else
                Accountant.Config.PlotNames.Remove(change.Value);
            Accountant.Config.Save();
            _timerWindow.ResetCropCache();
        }
    }
}
