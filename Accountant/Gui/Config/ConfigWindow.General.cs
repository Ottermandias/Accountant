using System;
using Accountant.Classes;
using Accountant.Gui.Helper;
using Accountant.Gui.Timer;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private void DrawConfigTab()
    {
        if (!ImGui.BeginTabItem("Config##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        if (!ImGui.BeginChild("##GeneralTab"))
            return;

        raii.Push(ImGui.EndChild);

        ImGuiRaii.ConfigCheckmark("Enable Plugin",                  Accountant.Config.Enabled,              EnableTimers);
        ImGuiRaii.ConfigCheckmark("Show Timers",                    Accountant.Config.WindowVisible,        b => Accountant.Config.WindowVisible        = b);
        ImGuiRaii.ConfigCheckmark("No Collapsed Header Styling",    Accountant.Config.NoHeaderStyling,      b => Accountant.Config.NoHeaderStyling      = b);
        ImGuiRaii.ConfigCheckmark("No Timer Window Resize",         Accountant.Config.ProhibitResize,       b => Accountant.Config.ProhibitResize       = b);
        ImGuiRaii.ConfigCheckmark("No Timer Window Movement",       Accountant.Config.ProhibitMoving,       b => Accountant.Config.ProhibitMoving       = b);
        ImGuiRaii.ConfigCheckmark("Hide Disabled Objects",          Accountant.Config.HideDisabled,         b => Accountant.Config.HideDisabled         = b);
        ImGuiRaii.HoverTooltip("Hide objects that are disabled or limited from the timers.");
        ImGuiRaii.ConfigCheckmark("Always Show Characters' Worlds", Accountant.Config.ShowCharacterWorlds,  ShowCharacterWorlds);
        ImGui.NewLine();

        ImGuiRaii.ConfigCheckmark("Enable Retainer Timers", Accountant.Config.EnableRetainers, EnableRetainers);
        ImGui.NewLine();
        ImGuiRaii.ConfigCheckmark("Enable Airship Timers",         Accountant.Config.EnableAirships,     EnableAirships);
        ImGuiRaii.ConfigCheckmark("Enable Submersible Timers",     Accountant.Config.EnableSubmersibles, EnableSubmersibles);
        ImGuiRaii.ConfigCheckmark("Enable Aetherial Wheel Timers", Accountant.Config.EnableWheels,       EnableWheels);
        ImGui.NewLine();
        ImGuiRaii.ConfigCheckmark("Enable Crop Timers",        Accountant.Config.EnableCrops,        EnableCrops);
        ImGuiRaii.ConfigCheckmark("Ignore Indoor Plot Plants", Accountant.Config.IgnoreIndoorPlants, IgnoreIndoorPlants);
        ImGuiRaii.ConfigCheckmark("Group Crop Beds by Plant",  Accountant.Config.OrderByCrop,        OrderByCrop);
        ImGui.NewLine();
        ImGuiRaii.ConfigCheckmark("Enable Leve Allowance Timers", Accountant.Config.EnableLeveAllowances, EnableLeveAllowances);
        DrawLeveAllowancesWarningInput();
        ImGuiRaii.ConfigCheckmark("Enable Squadron Mission Timers", Accountant.Config.EnableSquadron,     EnableSquadron);
        ImGuiRaii.ConfigCheckmark("Enable Map Allowance Timers",    Accountant.Config.EnableMapAllowance, EnableMapAllowance);
        ImGuiRaii.ConfigCheckmark("Enable Mini Cactpot Timers",     Accountant.Config.EnableMiniCactpot,  EnableMiniCactpot);
        ImGuiRaii.ConfigCheckmark("Enable Jumbo Cactpot Timers",    Accountant.Config.EnableJumboCactpot, EnableJumboCactpot);
        ImGui.NewLine();
    }

    private static void DrawColorsTab()
    {
        if (!ImGui.BeginTabItem("Colors##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        if (!ImGui.BeginChild("##ColorsTab"))
            return;

        raii.Push(ImGui.EndChild);

        foreach (var color in Enum.GetValues<ColorId>())
        {
            ImGuiRaii.ConfigColorPicker(color.Name(), color.Description(), color.Value(), c => Accountant.Config.Colors[color] = c,
                color.Default());
        }
    }

    private void DrawLeveAllowancesWarningInput()
    {
        var leveAllowances = Accountant.Config.LeveWarning;
        ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
        if (!ImGui.DragInt("Leve Allowances Warning", ref leveAllowances, 1, 0, Leve.AllowanceError))
            return;

        if (leveAllowances < 0)
            leveAllowances = 0;
        if (leveAllowances > Leve.AllowanceError)
            leveAllowances = Leve.AllowanceError;
        if (leveAllowances == Accountant.Config.LeveWarning)
            return;

        Accountant.Config.LeveWarning = leveAllowances;
        Accountant.Config.Save();
        _timerWindow.ResetCache(typeof(TimerWindow.TaskCache));
    }

    private void OrderByCrop(bool toggle)
    {
        Accountant.Config.OrderByCrop = toggle;
        _timerWindow.ResetCache(typeof(TimerWindow.CropCache));
    }

    private void IgnoreIndoorPlants(bool toggle)
    {
        Accountant.Config.IgnoreIndoorPlants = toggle;
        _timerWindow.ResetCache(typeof(TimerWindow.CropCache));
    }

    private void EnableCache(bool toggle, ConfigFlags flag, Type type)
    {
        Accountant.Config.Flags.Set(flag, toggle);
        _timers.CheckSettings();
        _timerWindow.ResetCache(type);
    }

    private void ShowCharacterWorlds(bool toggle){
        Accountant.Config.ShowCharacterWorlds = toggle;
        _timerWindow.ResetCache(typeof(TimerWindow.RetainerCache));
        _timerWindow.ResetCache(typeof(TimerWindow.TaskCache));
    }

    private void EnableRetainers(bool toggle)
        => EnableCache(toggle, ConfigFlags.Retainers, typeof(TimerWindow.RetainerCache));

    private void EnableAirships(bool toggle)
        => EnableCache(toggle, ConfigFlags.Airships, typeof(TimerWindow.MachineCache));

    private void EnableSubmersibles(bool toggle)
        => EnableCache(toggle, ConfigFlags.Submersibles, typeof(TimerWindow.MachineCache));

    private void EnableWheels(bool toggle)
        => EnableCache(toggle, ConfigFlags.AetherialWheels, typeof(TimerWindow.WheelCache));

    private void EnableCrops(bool toggle)
        => EnableCache(toggle, ConfigFlags.Crops, typeof(TimerWindow.CropCache));

    private void EnableLeveAllowances(bool toggle)
        => EnableCache(toggle, ConfigFlags.LeveAllowances, typeof(TimerWindow.TaskCache));

    private void EnableSquadron(bool toggle)
        => EnableCache(toggle, ConfigFlags.Squadron, typeof(TimerWindow.TaskCache));

    private void EnableMapAllowance(bool toggle)
        => EnableCache(toggle, ConfigFlags.MapAllowance, typeof(TimerWindow.TaskCache));

    private void EnableMiniCactpot(bool toggle)
        => EnableCache(toggle, ConfigFlags.MiniCactpot, typeof(TimerWindow.TaskCache));

    private void EnableJumboCactpot(bool toggle)
        => EnableCache(toggle, ConfigFlags.JumboCactpot, typeof(TimerWindow.TaskCache));

    private void EnableTimers(bool toggle)
        => EnableCache(toggle, ConfigFlags.Enabled, typeof(TimerWindow.BaseCache));
}
