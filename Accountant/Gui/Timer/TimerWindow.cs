using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Accountant.Manager;
using Dalamud.Interface;
using ImGuiNET;
using OtterLoc.Structs;
using DateTime = System.DateTime;

namespace Accountant.Gui.Timer;

public partial class TimerWindow : IDisposable
{
    private readonly float       _widthTotal;
    private readonly string      _completedString;
    private readonly string      _availableString;
    private readonly IconStorage _icons        = new(64);
    private          string      _headerString = "Timers###Accountant.Timers";
    private          ColorId     _headerColor  = ColorId.NeutralHeader;
    private          bool        _drawData     = true;

    private readonly BaseCache[] _cache;

    private DateTime _now = DateTime.UtcNow;

    private readonly CropCache     _cropCache;
    private readonly RetainerCache _retainerCache;
    private readonly MachineCache  _machineCache1;
    private readonly MachineCache  _machineCache2;

    public TimerWindow(TimerManager manager)
    {
        _cache = manager.CreateCaches(this);
        SortCache();
        _cropCache       = (CropCache)_cache.First(c => c is CropCache);
        _retainerCache   = (RetainerCache)_cache.First(c => c is RetainerCache);
        _machineCache1   = (MachineCache)_cache.First(c => c is MachineCache);
        _machineCache2   = (MachineCache)_cache.Last(c => c is MachineCache);
        _completedString = StringId.Completed.Value();
        _availableString = StringId.Available.Value();

        var maxWidth = Math.Max(ImGui.CalcTextSize(_completedString).X, ImGui.CalcTextSize(_availableString).X);
        maxWidth = Math.Max(maxWidth, ImGui.CalcTextSize("00:00:00").X);

        var widthTime = maxWidth / ImGuiHelpers.GlobalScale;
        _widthTotal = ImGui.CalcTextSize("mmmmmmmmmmmmmmmmmmmm").X / ImGuiHelpers.GlobalScale
          + widthTime
          + ImGui.GetStyle().ScrollbarSize / ImGuiHelpers.GlobalScale;

        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
    }

    public void SortCache()
        => Array.Sort(_cache, (l, r) => Accountant.Config.GetPriority(r.Name).CompareTo(Accountant.Config.GetPriority(l.Name)));

    public void Dispose()
    {
        Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        _icons.Dispose();
    }

    public void ResetCache()
    {
        foreach (var cache in _cache)
            cache.Resetter();
    }

    public void ResetCache(Type type)
    {
        foreach (var cache in _cache.Where(c => c.GetType() == type))
            cache.Resetter();
    }

    public void ResetCache(Type type1, Type type2)
    {
        foreach (var cache in _cache.Where(c => c.GetType() == type1 || c.GetType() == type2))
            cache.Resetter();
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
                .Push(ImGuiCol.TitleBgCollapsed, _headerColor.Value());
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
            foreach (var cache in _cache)
                cache.Draw(_now);

            if (Accountant.Config.Flags.Check(ConfigFlags.Retainers))
                _headerString = Accountant.Config.Flags.Any(ConfigFlags.Airships | ConfigFlags.Submersibles)
                    ? $"{_retainerCache.Header}    {(_machineCache1.GlobalCounter + _machineCache2.GlobalCounter).GetHeader(StringId.Machines)}###Accountant.Timers"
                    : $"{_retainerCache.Header}###Accountant.Timers";
            else if (Accountant.Config.Flags.Any(ConfigFlags.Airships | ConfigFlags.Submersibles))
                _headerString =
                    $"{(_machineCache1.GlobalCounter + _machineCache2.GlobalCounter).GetHeader(StringId.Machines)}###Accountant.Timers";

            if (Accountant.Config.Flags.Check(ConfigFlags.Crops))
                _headerColor = _cropCache.Color;
        }
        finally
        {
            ImGui.End();
        }
    }

    internal static string TimeSpanString(TimeSpan span, int align = 2)
        => $"{((int)span.TotalHours).ToString(align == 2 ? "D2" : "D3")}:{span.Minutes:D2}:{span.Seconds:D2}";

    private string? StatusString(ObjectStatus status)
        => status switch
        {
            ObjectStatus.Available => _availableString,
            ObjectStatus.Completed => _completedString,
            ObjectStatus.Sent      => null,
            ObjectStatus.Limited   => "Limited",
            _                      => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
}
