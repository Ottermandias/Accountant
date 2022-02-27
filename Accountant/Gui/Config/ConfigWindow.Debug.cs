using System.Diagnostics;
using Accountant.Gui.Helper;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    [Conditional("DEBUG")]
    private void DrawDebugTab()
    {
        if (!ImGui.BeginTabItem("Debug##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);
        if (!ImGui.BeginTable("##debugtable", 2))
            return;

        raii.Push(ImGui.EndTable);

        var       info = _timers.CompanyStorage.GetCurrentCompanyInfo();
        ImGui.TableNextColumn();
        ImGui.Text("Free Company Name");
        ImGui.TableNextColumn();
        ImGui.Text(info?.Name ?? "Unknown");

        ImGui.TableNextColumn();
        ImGui.Text("Free Company Tag");
        ImGui.TableNextColumn();
        ImGui.Text(info?.Tag ?? "Unknown");

        ImGui.TableNextColumn();
        ImGui.Text("Free Company Leader");
        ImGui.TableNextColumn();
        ImGui.Text(info?.Leader ?? "Unknown");

        ImGui.TableNextColumn();
        ImGui.Text("Free Company Server Id");
        ImGui.TableNextColumn();
        ImGui.Text(info?.ServerId.ToString() ?? "Unknown");

        ImGui.TableNextColumn();
        ImGui.Text("Current Territory");
        ImGui.TableNextColumn();
        ImGui.Text(_timers.PositionInfo.Zone.ToString());

        ImGui.TableNextColumn();
        ImGui.Text("Current Ward");
        ImGui.TableNextColumn();
        ImGui.Text($"{_timers.PositionInfo.Ward}{(_timers.PositionInfo.Subdivision ? " (Subdivision)" : string.Empty)}");

        ImGui.TableNextColumn();
        ImGui.Text("Current Plot");
        ImGui.TableNextColumn();
        ImGui.Text(_timers.PositionInfo.Plot.ToString());

        ImGui.TableNextColumn();
        ImGui.Text("Current House");
        ImGui.TableNextColumn();
        ImGui.Text(_timers.PositionInfo.House.ToString());
    }
}
