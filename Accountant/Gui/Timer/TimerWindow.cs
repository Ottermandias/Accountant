using System;
using System.Linq;
using System.Numerics;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Accountant.Manager;
using Accountant.Util;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Events.EventDataTypes;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using OtterLoc.Structs;
using DateTime = System.DateTime;

namespace Accountant.Gui.Timer;

public partial class TimerWindow : IDisposable
{
    private          float   _widthTotal;
    private readonly string  _completedString;
    private readonly string  _availableString;
    private          string  _headerString = "Timers###Accountant.Timers";
    private          ColorId _headerColor  = ColorId.NeutralHeader;
    private          bool    _drawData     = true;

    private readonly BaseCache[] _cache;

    private DateTime _now = DateTime.UtcNow;

    private readonly CropCache          _cropCache;
    private readonly RetainerCache      _retainerCache;
    private readonly MachineCache       _subCache;
    private readonly MachineCache       _airshipCache;
    private readonly DemolitionWarning  _demolitionWarning;
    private readonly FreeCompanyStorage _freeCompany;

    internal static DtrManager DtrManager = null!;

    public TimerWindow(TimerManager manager, DemolitionManager demolitionManager, FreeCompanyStorage freeCompany)
    {
        _freeCompany       = freeCompany;
        _demolitionWarning = new DemolitionWarning(demolitionManager, Dalamud.Framework);
        _cache             = manager.CreateCaches(this);
        SortCache();
        _cropCache       = (CropCache)_cache.First(c => c is CropCache);
        _retainerCache   = (RetainerCache)_cache.First(c => c is RetainerCache);
        _subCache        = (MachineCache)_cache.First(c => c is MachineCache);
        _airshipCache    = (MachineCache)_cache.Last(c => c is MachineCache);
        _completedString = StringId.Completed.Value();
        _availableString = StringId.Available.Value();

        DtrManager = new DtrManager(Dalamud.Dtr, _cropCache, _retainerCache, _airshipCache, _subCache);
        if (Accountant.Config.ShowDtr)
            DtrManager.Enable();

        Dalamud.PluginInterface.UiBuilder.Draw       += Draw;
        Dalamud.PluginInterface.UiBuilder.OpenMainUi += Toggle;
    }

    public static void Toggle()
    {
        Accountant.Config.WindowVisible = !Accountant.Config.WindowVisible;
        Accountant.Config.Save();
    }


    public static void Toggle(DtrInteractionEvent obj)
        => Toggle();

    private void SetWidthTotal()
    {
        if (_widthTotal > 0)
            return;

        var maxWidth = Math.Max(ImGui.CalcTextSize(_completedString).X, ImGui.CalcTextSize(_availableString).X);
        maxWidth = Math.Max(maxWidth, ImGui.CalcTextSize("00:00:00").X);

        var widthTime = maxWidth / ImGuiHelpers.GlobalScale;
        _widthTotal = ImGui.CalcTextSize("mmmmmmmmmmmmmmmmmmmm").X / ImGuiHelpers.GlobalScale
          + widthTime
          + ImGui.GetStyle().ScrollbarSize / ImGuiHelpers.GlobalScale;
    }

    public void SortCache()
        => Array.Sort(_cache, (l, r) => Accountant.Config.GetPriority(r.Name).CompareTo(Accountant.Config.GetPriority(l.Name)));

    public void Dispose()
    {
        _demolitionWarning.Dispose();
        Dalamud.PluginInterface.UiBuilder.Draw       -= Draw;
        Dalamud.PluginInterface.UiBuilder.OpenMainUi -= Toggle;
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

    private void Draw()
    {
        if (!Accountant.Config.Enabled)
            return;

        _now = DateTime.UtcNow;

        if (!Accountant.Config.WindowVisible)
        {
            DtrManager.Update(true);
            return;
        }

        var flags = ImGuiWindowFlags.None;
        if (Accountant.Config.ProhibitMoving)
            flags |= ImGuiWindowFlags.NoMove;
        if (Accountant.Config.ProhibitResize)
            flags |= ImGuiWindowFlags.NoResize;

        SetWidthTotal();
        var minSize = new Vector2((Accountant.Config.FixedWindowWidth ?? 100) * ImGuiHelpers.GlobalScale,
            ImGui.GetFrameHeightWithSpacing() * 2 + ImGui.GetStyle().ItemSpacing.Y);
        var maxSize = new Vector2((Accountant.Config.FixedWindowWidth ?? 1000) * ImGuiHelpers.GlobalScale, 100000);
        ImGui.SetNextWindowSizeConstraints(minSize, maxSize);

        var enabled = Accountant.Config.WindowVisible;
        using var colors = ImGuiRaii.PushColor(ImGuiCol.WindowBg, ColorId.Background.Value())
            .Push(ImGuiCol.Text, ColorId.NeutralText.Value());
        if (!_drawData && !Accountant.Config.NoHeaderStyling)
        {
            colors.Push(ImGuiCol.Border, ColorId.CollapsedBorder.Value())
                .Push(ImGuiCol.TitleBgCollapsed, _headerColor.Value());
            using var style = ImGuiRaii.PushStyle(ImGuiStyleVar.WindowBorderSize, 1);
            _drawData = ImGui.Begin(_headerString, ref enabled, flags);
            colors.Pop(2);
        }
        else
        {
            _drawData = ImGui.Begin(_headerString, ref enabled, flags);
        }

        if (!enabled)
        {
            Accountant.Config.WindowVisible = false;
            Accountant.Config.Save();
        }

        try
        {
            DrawCompanyWarning();
            DrawDemolition();

            foreach (var cache in _cache)
                cache.Draw(_now);

            if (Accountant.Config.Flags.Check(ConfigFlags.Retainers))
                _headerString = Accountant.Config.Flags.Any(ConfigFlags.Airships | ConfigFlags.Submersibles)
                    ? $"{_retainerCache.Counter.GetHeader(StringId.Retainers)}    {(_subCache.GlobalCounter + _airshipCache.GlobalCounter).GetHeader(StringId.Machines)}###Accountant.Timers"
                    : $"{_retainerCache.Counter.GetHeader(StringId.Retainers)}###Accountant.Timers";
            else if (Accountant.Config.Flags.Any(ConfigFlags.Airships | ConfigFlags.Submersibles))
                _headerString =
                    $"{(_subCache.GlobalCounter + _airshipCache.GlobalCounter).GetHeader(StringId.Machines)}###Accountant.Timers";

            if (Accountant.Config.Flags.Check(ConfigFlags.Crops))
                _headerColor = _cropCache.Color;
            DtrManager.Update(false);
        }
        finally
        {
            ImGui.End();
        }
    }

    private void DrawCompanyWarning()
    {
        if (!Accountant.Config.ShowFreeCompanyWarning)
            return;

        var info           = _freeCompany.GetCurrentCompanyInfo();
        var infoIncomplete = !info.HasValue || info.Value.Tag.Length > 0 && info.Value.Name.IsNullOrEmpty();
        var tagUnavailable = Accountant.GameData.FreeCompanyInfo().Tag.IsNullOrEmpty();
        if (!infoIncomplete || tagUnavailable)
            return;

        using var c = ImGuiRaii.PushColor(ImGuiCol.Header, _demolitionWarning.HeaderColor.Value());
        ImGui.CollapsingHeader("No Full Free Company Data", ImGuiTreeNodeFlags.DefaultOpen);
        if (ImGui.IsItemHovered())
        {
            using var tt = ImRaii.Tooltip();
            ImGui.TextWrapped("No full data about your current free company available.\n\n"
              + "Please leave your current instance or relog onto your character if this does not help to obtain Free Company Data after a Accountant Update.\n\n"
              + "You can disable this warning permanently in the settings.");
        }
    }

    private void DrawDemolition()
    {
        if (_demolitionWarning.Warnings.Count <= 0)
            return;

        using var c = ImGuiRaii.PushColor(ImGuiCol.Header, _demolitionWarning.HeaderColor.Value());
        if (!ImGui.CollapsingHeader("Demolishing Houses", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        foreach (var warning in _demolitionWarning.Warnings)
        {
            c.Push(ImGuiCol.Text, warning.Color.Value());
            if (ImGui.TreeNodeEx(warning.Name, ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.Leaf))
                ImGui.TreePop();
            ImGui.SameLine();
            var size = ImGui.CalcTextSize(warning.Status);
            ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - size.X);
            ImGui.TextUnformatted(warning.Status);
            c.Pop();
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
