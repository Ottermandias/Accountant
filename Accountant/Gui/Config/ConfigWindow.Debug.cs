using System;
using System.Diagnostics;
using Accountant.Enums;
using Accountant.Gui.Helper;
using ImGuiNET;
using OtterLoc.Structs;

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

        var info = _timers.CompanyStorage.GetCurrentCompanyInfo();

        ImGui.TableNextColumn();
        ImGui.Text("Current House");
        ImGui.TableNextColumn();
        ImGui.Text(_demoManager.CurrentPlot.ToName());

        ImGui.TableNextColumn();
        ImGui.Text("Demolition Subscribed");
        ImGui.TableNextColumn();
        ImGui.Text(_demoManager.FrameworkSubscribed.ToString());

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Test Demolition");
        ImGui.TableNextColumn();
        if (ImGui.SmallButton("Test##Demolition"))
            _demoManager.Test();

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

        foreach (var id in Enum.GetValues<StringId>())
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(id.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(id.Value());
        }
    }
}
