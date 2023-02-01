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

        ImGuiRaii.ConfigCheckmark("Enable Plugin",               Accountant.Config.Enabled,         EnableTimers);
        ImGuiRaii.ConfigCheckmark("Show Timers",                 Accountant.Config.WindowVisible,   b => Accountant.Config.WindowVisible   = b);
        ImGuiRaii.ConfigCheckmark("No Collapsed Header Styling", Accountant.Config.NoHeaderStyling, b => Accountant.Config.NoHeaderStyling = b);
        ImGuiRaii.ConfigCheckmark("No Timer Window Resize",      Accountant.Config.ProhibitResize,  b => Accountant.Config.ProhibitResize  = b);
        ImGuiRaii.ConfigCheckmark("No Timer Window Movement",    Accountant.Config.ProhibitMoving,  b => Accountant.Config.ProhibitMoving  = b);
        ImGuiRaii.ConfigCheckmark("Hide Disabled Objects",       Accountant.Config.HideDisabled,    b => Accountant.Config.HideDisabled    = b);
        ImGuiRaii.ConfigCheckmark("Hide Timers During Combat",   Accountant.Config.NoTimerWindowInCombat,       b => Accountant.Config.NoTimerWindowInCombat = b);
        ImGuiRaii.ConfigCheckmark("Hide Timers In Instance",     Accountant.Config.NoTimerWindowInInstance,     b => Accountant.Config.NoTimerWindowInInstance = b);
        ImGuiRaii.ConfigCheckmark("Hide Timers During Cutscene", Accountant.Config.NoTimerWindowDuringCutscene, b => Accountant.Config.NoTimerWindowDuringCutscene = b);
        ImGuiRaii.HoverTooltip("Hide objects that are disabled or limited from the timers.");
        ImGui.NewLine();

        ImGuiRaii.ConfigCheckmark("Enable Retainer Timers", Accountant.Config.EnableRetainers, EnableRetainers);
        ImGui.NewLine();
        ImGuiRaii.ConfigCheckmark("Enable Airship Timers",         Accountant.Config.EnableAirships,     EnableAirships);
        ImGuiRaii.ConfigCheckmark("Enable Submersible Timers",     Accountant.Config.EnableSubmersibles, EnableSubmersibles);
        ImGuiRaii.ConfigCheckmark("Enable Aetherial Wheel Timers", Accountant.Config.EnableWheels,       EnableWheels);
        ImGui.NewLine();
        ImGuiRaii.ConfigCheckmark("Enable Crop Timers",        Accountant.Config.EnableCrops, EnableCrops);
        ImGuiRaii.ConfigCheckmark("Ignore Indoor Plot Plants", Accountant.Config.IgnoreIndoorPlants, IgnoreIndoorPlants);
        ImGuiRaii.ConfigCheckmark("Group Crop Beds by Plant",  Accountant.Config.OrderByCrop, OrderByCrop);
        ImGuiRaii.ConfigCheckmark("Show Ward-Update Tooltip",  Accountant.Config.ShowCropTooltip, v => Accountant.Config.ShowCropTooltip = v);
        ImGui.NewLine();
        ImGuiRaii.ConfigCheckmark("Enable Leve Allowance Timers", Accountant.Config.EnableLeveAllowances, EnableLeveAllowances);
        DrawLeveAllowancesWarningInput();
        ImGuiRaii.ConfigCheckmark("Enable Squadron Mission Timers",          Accountant.Config.EnableSquadron,     EnableSquadron);
        ImGuiRaii.ConfigCheckmark("Enable Map Allowance Timers",             Accountant.Config.EnableMapAllowance, EnableMapAllowance);
        ImGuiRaii.ConfigCheckmark("Enable Mini Cactpot Timers",              Accountant.Config.EnableMiniCactpot,  EnableMiniCactpot);
        ImGuiRaii.ConfigCheckmark("Enable Jumbo Cactpot Timers",             Accountant.Config.EnableJumboCactpot, EnableJumboCactpot);
        ImGuiRaii.ConfigCheckmark("Enable Custom Delivery Allowance Timers", Accountant.Config.EnableDeliveries,   EnableDeliveries);
        ImGuiRaii.ConfigCheckmark("Enable Tribe Allowance Timers",           Accountant.Config.EnableTribes,       EnableTribes);
        DrawTribeAllowancesFinishedInput();
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

    private void DrawTribeAllowancesFinishedInput()
    {
        var tribeAllowances = Accountant.Config.TribesFinished;
        ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
        if (!ImGui.DragInt("Tribe Quests Finished", ref tribeAllowances, 1, 0, Tribe.AllowanceCap))
            return;

        if (tribeAllowances < 0)
            tribeAllowances = 0;
        if (tribeAllowances > Tribe.AllowanceCap)
            tribeAllowances = Tribe.AllowanceCap;
        if (tribeAllowances == Accountant.Config.TribesFinished)
            return;

        Accountant.Config.LeveWarning = tribeAllowances;
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

    private void EnableDeliveries(bool toggle)
        => EnableCache(toggle, ConfigFlags.CustomDelivery, typeof(TimerWindow.TaskCache));

    private void EnableTribes(bool toggle)
        => EnableCache(toggle, ConfigFlags.Tribes, typeof(TimerWindow.TaskCache));

    private void EnableTimers(bool toggle)
        => EnableCache(toggle, ConfigFlags.Enabled, typeof(TimerWindow.BaseCache));
}
