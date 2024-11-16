using System;
using System.Diagnostics;
using System.Globalization;
using Accountant.Enums;
using Dalamud.Interface.Utility.Raii;
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

        DrawCompanyInfo();
        DrawPositionInfo();
        DrawSquadron();
        DrawFreeCompanyStorage();
        DrawTests();
        DrawAddresses();
        DrawStrings();
    }

    private void DrawFreeCompanyStorage()
    {
        if (!ImGui.CollapsingHeader("Free Company Storage Data"))
            return;

        ImGui.TextUnformatted($"Last Change: {_freeCompanyStorage.LastChangeTime}");
        foreach (var company in _freeCompanyStorage.Infos)
            ImGui.BulletText($"{company.Name} <{company.Tag}> by {company.Leader}@{Accountant.GameData.GetWorldName(company.ServerId)}");
    }

    private static void DrawStrings()
    {
        if (!ImGui.CollapsingHeader("Strings"))
            return;

        using var table = ImRaii.Table("##debugtableStrings", 2);
        if (!table)
            return;

        foreach (var id in Enum.GetValues<StringId>())
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(id.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(id.Value());
        }
    }

    private void DrawAddresses()
    {
        if (!ImGui.CollapsingHeader("Addresses", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        using var table = ImRaii.Table("##debugtableAddresses", 2);
        if (!table)
            return;

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Position Info");
        ImGui.TableNextColumn();
        CopyOnClickSelectable(Interop.PositionInfo.Address);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Squadron Container");
        ImGui.TableNextColumn();
        CopyOnClickSelectable(Interop.SquadronContainer.Address);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Update Gold Saucer Data");
        ImGui.TableNextColumn();
        CopyOnClickSelectable(Interop.UpdateGoldSaucerData.Address);
    }

    private void DrawPositionInfo()
    {
        if (!ImGui.CollapsingHeader("Position", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        using var table = ImRaii.Table("##debugtablePosition", 2);
        if (!table)
            return;

        ImGui.TableNextColumn();
        ImGui.Text("Current Clientstate Territory ID");
        ImGui.TableNextColumn();
        ImGui.Text(Dalamud.ClientState.TerritoryType.ToString());

        ImGui.TableNextColumn();
        ImGui.Text("Current House");
        ImGui.TableNextColumn();
        ImGui.Text(_demoManager.CurrentPlot.ToName());

        ImGui.TableNextColumn();
        ImGui.Text("Current Housing Territory");
        ImGui.TableNextColumn();
        ImGui.Text(Interop.PositionInfo.Zone.ToString());

        ImGui.TableNextColumn();
        ImGui.Text("Current Ward");
        ImGui.TableNextColumn();
        ImGui.Text($"{Interop.PositionInfo.Ward}{(Interop.PositionInfo.Subdivision ? " (Subdivision)" : string.Empty)}");

        ImGui.TableNextColumn();
        ImGui.Text("Current Plot");
        ImGui.TableNextColumn();
        ImGui.Text(Interop.PositionInfo.Plot.ToString());

        ImGui.TableNextColumn();
        ImGui.Text("Current House");
        ImGui.TableNextColumn();
        ImGui.Text(Interop.PositionInfo.House.ToString());
    }

    private void DrawSquadron()
    {
        if (!ImGui.CollapsingHeader("Squadron", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        using var table = ImRaii.Table("##debugtableSquadron", 2);
        if (!table)
            return;

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Mission ID");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Interop.SquadronContainer.MissionId.ToString());

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Mission End");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Interop.SquadronContainer.MissionEnd.ToString(CultureInfo.InvariantCulture));

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Training ID");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Interop.SquadronContainer.TrainingId.ToString());

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Training End");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Interop.SquadronContainer.TrainingEnd.ToString(CultureInfo.InvariantCulture));

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("New Recruits");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Interop.SquadronContainer.NewRecruits.ToString());
    }

    private void DrawTests()
    {
        if (!ImGui.CollapsingHeader("Tests", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        using var table = ImRaii.Table("##debugtableTests", 2);
        if (!table)
            return;

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
        ImGui.TextUnformatted("Clear FC Data");
        ImGui.TableNextColumn();
        if (ImGui.SmallButton("Clear##FCData"))
        {
            _freeCompanyStorage.Infos.Clear();
            _freeCompanyStorage.Save();
        }
    }

    private void DrawCompanyInfo()
    {
        if (!ImGui.CollapsingHeader("Free Company", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        using var table = ImRaii.Table("##debugtableCompany", 2);
        if (!table)
            return;

        var info = _timers.CompanyStorage.GetCurrentCompanyInfo();
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
    }

    private static void CopyOnClickSelectable(string text)
    {
        if (ImGui.Selectable(text))
            ImGui.SetClipboardText(text);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"Click to copy to clipboard.");
    }

    private static void CopyOnClickSelectable(nint ptr)
        => CopyOnClickSelectable($"0x{ptr:X}");
}
