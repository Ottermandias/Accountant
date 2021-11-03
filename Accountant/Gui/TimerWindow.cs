using System;
using System.Numerics;
using System.Reflection.Metadata;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Accountant.Manager;
using Dalamud.Interface;
using ImGuiNET;
using OtterLoc.Structs;
using DateTime = System.DateTime;

namespace Accountant.Gui;

public partial class TimerWindow
{ }

public partial class TimerWindow : IDisposable
{
    private readonly TimerManager _manager;

    private readonly float       _widthTotal;
    private readonly float       _widthShortTime;
    private readonly float       _widthTime;
    private readonly string      _completedString;
    private readonly string      _availableString;
    private readonly IconStorage _icons        = new(64);
    private          string      _headerString = "Timers###Accountant.Timers";
    private          bool        _drawData     = true;

    private DateTime _now = DateTime.UtcNow;

    public TimerWindow(TimerManager manager)
    {
        _manager         = manager;
        _retainerCache   = new RetainerCache(this, manager);
        _machineCache    = new MachineCache(this, manager);
        _cropCache       = new CropCache(this, manager);
        _completedString = StringId.Completed.Value();
        _availableString = StringId.Available.Value();

        var maxWidth = Math.Max(ImGui.CalcTextSize(_completedString).X, ImGui.CalcTextSize(_availableString).X);
        maxWidth = Math.Max(maxWidth, ImGui.CalcTextSize("00:00:00").X);

        _widthTime      = maxWidth / ImGuiHelpers.GlobalScale;
        _widthShortTime = maxWidth;
        _widthTotal = ImGui.CalcTextSize("mmmmmmmmmmmmmmmmmmmm").X / ImGuiHelpers.GlobalScale
          + _widthTime
          + ImGui.GetStyle().ScrollbarSize / ImGuiHelpers.GlobalScale;

        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
    }

    public void Dispose()
    {
        Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        _icons.Dispose();
    }

    private void Draw()
    {
        if (!Accountant.Config.Enabled || !Accountant.Config.WindowVisible)
            return;

        _now = DateTime.UtcNow;

        var minSize = new Vector2(_widthTotal * ImGuiHelpers.GlobalScale,
            ImGui.GetFrameHeightWithSpacing() * 4 + ImGui.GetStyle().ItemSpacing.Y * 3);
        var maxSize = new Vector2(minSize.X, 100000);
        ImGui.SetNextWindowSizeConstraints(minSize, maxSize);

        var enabled = Accountant.Config.WindowVisible;
        using var colors = ImGuiRaii.PushColor(ImGuiCol.WindowBg, ColorId.Background.Value())
            .Push(ImGuiCol.Text, ColorId.NeutralText.Value());
        if (!_drawData)
        {
            colors.Push(ImGuiCol.Border, ColorId.CollapsedBorder.Value())
                .Push(ImGuiCol.TitleBgCollapsed, _cropCache.GlobalColor.Value());
            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.WindowBorderSize, 1);
            _drawData = ImGui.Begin(_headerString, ref enabled);
            colors.Pop(2);
        }
        else
        {
            _drawData = ImGui.Begin(_headerString, ref enabled);
        }

        if (!enabled)
        {
            Accountant.Config.WindowVisible = false;
            Accountant.Config.Save();
        }

        try
        {
            DrawCrops();
            DrawRetainers();
            DrawMachines();

            _headerString = $"{_retainerCache.Header}    {_machineCache.Header}###Accountant.Timers";
        }
        finally
        {
            ImGui.End();
        }
    }
}
