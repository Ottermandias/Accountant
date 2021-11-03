using System;
using System.Diagnostics;
using Accountant.Gui.Helper;
using ImGuiNET;
using Lumina.Data.Parsing.Layer;

namespace Accountant.Gui;

public class ConfigWindow : IDisposable
{
    private readonly string _header;
    private          bool   _enabled = false;

    public ConfigWindow()
    {
        _header = Accountant.Version != string.Empty ? $"Accountant v{Accountant.Version}###Accountant" : "Accountant###Accountant";
        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi += Toggle;
    }

    public void Dispose()
    {
        Dalamud.PluginInterface.UiBuilder.Draw         -= Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= Toggle;
    }

    public void Toggle()
        => _enabled = !_enabled;

    private void DrawConfigTab()
    {
        if (!ImGui.BeginTabItem("Config##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        ImGuiRaii.ConfigCheckmark("Enable Plugin", Accountant.Config.Enabled,       b => Accountant.Config.Enabled       = b);
        ImGuiRaii.ConfigCheckmark("Show Timers",   Accountant.Config.WindowVisible, b => Accountant.Config.WindowVisible = b);
        ImGui.NewLine();

        ImGuiRaii.ConfigCheckmark("Enable Retainer Timers", Accountant.Config.EnableRetainers, b => Accountant.Config.EnableRetainers = b);
        ImGuiRaii.ConfigCheckmark("Enable Machine Timers",  Accountant.Config.EnableMachines,  b => Accountant.Config.EnableMachines  = b);
        ImGuiRaii.ConfigCheckmark("Enable Crop Timers",     Accountant.Config.EnableCrops,     b => Accountant.Config.EnableCrops     = b);
        ImGui.NewLine();
    }

    private void DrawColorsTab()
    {
        if (!ImGui.BeginTabItem("Colors##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        foreach (var color in Enum.GetValues<ColorId>())
        {
            ImGuiRaii.ConfigColorPicker(color.Name(), color.Description(), color.Value(), c => Accountant.Config.Colors[color] = c,
                color.Default());
        }
    }

    private void DrawDebugTab()
    {
        if (!ImGui.BeginTabItem("Debug##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);
    }

    private void Draw()
    {
        if (!_enabled)
            return;

        if (!ImGui.Begin(_header, ref _enabled))
        {
            ImGui.End();
            return;
        }

        using var raii = ImGuiRaii.DeferredEnd(ImGui.End);

        if (!ImGui.BeginTabBar("##AccountantTabs"))
            return;

        raii.Push(ImGui.EndTabBar);

        DrawConfigTab();
        DrawColorsTab();
        DrawDebugTab();
    }
}
