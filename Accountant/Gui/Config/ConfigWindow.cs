using System;
using System.Numerics;
using Accountant.Gui.Helper;
using Accountant.Manager;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui.Config;

public partial class ConfigWindow : IDisposable
{
    private readonly        TimerManager      _timers;
    private readonly        Timer.TimerWindow _timerWindow;
    private readonly        string            _header;
    private                 bool              _enabled;
    private static readonly Vector2           MinSize = new(640, 480);


    public ConfigWindow(TimerManager timers, Timer.TimerWindow timerWindow)
    {
        _timers      = timers;
        _timerWindow = timerWindow;
        _header      = Accountant.Version != string.Empty ? $"Accountant v{Accountant.Version}###Accountant" : "Accountant###Accountant";

        Dalamud.PluginInterface.UiBuilder.Draw         += Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi += Toggle;
    }

    public void Dispose()
    {
        Dalamud.PluginInterface.UiBuilder.Draw         -= Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= Toggle;
    }

    public void Toggle()
        => _enabled = !_enabled;

    private void Draw()
    {
        if (!_enabled)
            return;

        ImGui.SetNextWindowSizeConstraints(MinSize * ImGuiHelpers.GlobalScale, MinSize * 5);
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
        DrawPlotNamesTab();
        DrawBlocklistsTab();
        DrawOrderNamesTab();
        DrawDeleteTab();
    }
}
