using System;
using Accountant.Gui.Helper;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private void DrawConfigTab()
    {
        if (!ImGui.BeginTabItem("Config##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        ImGuiRaii.ConfigCheckmark("Enable Plugin", Accountant.Config.Enabled,       EnableTimers);
        ImGuiRaii.ConfigCheckmark("Show Timers",   Accountant.Config.WindowVisible, b => Accountant.Config.WindowVisible = b);
        ImGui.NewLine();

        ImGuiRaii.ConfigCheckmark("Enable Retainer Timers",   Accountant.Config.EnableRetainers, EnableRetainers);
        ImGuiRaii.ConfigCheckmark("Enable Machine Timers",    Accountant.Config.EnableMachines,  EnableMachines);
        ImGuiRaii.ConfigCheckmark("Enable Crop Timers",       Accountant.Config.EnableCrops,     EnableCrops);
        ImGuiRaii.ConfigCheckmark("Group Crop Beds by Plant", Accountant.Config.OrderByCrop,     OrderByCrop);
    }

    private static void DrawColorsTab()
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

    private void OrderByCrop(bool toggle)
    {
        Accountant.Config.OrderByCrop = toggle;
        _timerWindow.ResetCropCache();
    }

    private void EnableRetainers(bool toggle)
    {
        Accountant.Config.EnableRetainers = toggle;
        _timers.EnableRetainers(toggle && Accountant.Config.Enabled, true);
        _timerWindow.Resubscribe(true, false, false);
    }

    private void EnableMachines(bool toggle)
    {
        Accountant.Config.EnableMachines = toggle;
        _timers.EnableMachines(toggle && Accountant.Config.Enabled, true);
        _timerWindow.Resubscribe(false, true, false);
    }

    private void EnableCrops(bool toggle)
    {
        Accountant.Config.EnableCrops = toggle;
        _timers.EnableCrops(toggle && Accountant.Config.Enabled, true);
        _timerWindow.Resubscribe(false, false, true);
    }

    private void EnableTimers(bool toggle)
    {
        Accountant.Config.Enabled = toggle;
        _timers.EnableRetainers(toggle && Accountant.Config.EnableRetainers, true);
        _timers.EnableMachines(toggle && Accountant.Config.EnableMachines, true);
        _timers.EnableCrops(toggle && Accountant.Config.EnableCrops, true);
        _timerWindow.Resubscribe(true, true, true);
    }
}
