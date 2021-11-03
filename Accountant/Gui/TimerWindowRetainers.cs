using System;
using System.Numerics;
using Accountant.Enums;
using Accountant.Gui.Helper;
using ImGuiNET;
using OtterLoc.Structs;

namespace Accountant.Gui;

public partial class TimerWindow
{
    public void Draw(CacheObject item)
    {
        using var color  = ImGuiRaii.PushColor(ImGuiCol.Text, item.Color.Value());
        using var indent = ImGuiRaii.PushStyle(ImGuiStyleVar.IndentSpacing, ImGui.GetStyle().IndentSpacing / 2);
        var       tree   = false;

        var tooltip = false;
        if (item.Children.Length == 0)
        {
            indent.Push(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing / 2);
            item.DrawIcon();
            ImGui.Selectable(item.Name);
            tooltip = ImGui.IsItemHovered();
            indent.Pop();
        }
        else
            tree = ImGui.TreeNodeEx(item.Name);

        if (item.DisplayString != null)
        {
            var width = ImGui.CalcTextSize(item.DisplayString).X;
            var display = item.DisplayString;
            ImGui.SameLine(ImGui.GetWindowContentRegionWidth() - width);
            ImGui.Text(display);
        }
        else if (item.DisplayTime != DateTime.MinValue)
        {
            var display = TimeSpanString(item.DisplayTime - _now);
            var width   = ImGui.CalcTextSize(display).X;
            ImGui.SameLine(ImGui.GetWindowContentRegionWidth() - width);
            ImGui.Text(display);
        }
        color.Pop();
        if (tooltip)
            item.DrawTooltip();

        if (tree)
        {
            foreach (var child in item.Children)
                Draw(child);
            ImGui.TreePop();
        }
    }


    private readonly RetainerCache _retainerCache;
    private readonly MachineCache  _machineCache;
    private readonly CropCache     _cropCache;
    private bool DrawHeader(string name, uint color, DateTime time)
    {
        using var c  = ImGuiRaii.PushColor(ImGuiCol.Header, color);
        var       posY    = ImGui.GetCursorPosY();
        var       header = ImGui.CollapsingHeader(name);
        if (time < _now)
            return header;

        var s     = TimeSpanString(time - _now);
        var width = ImGui.CalcTextSize(s).X;
        var pos   = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionWidth() - width, posY));
        ImGui.AlignTextToFramePadding();
        ImGui.Text(s);
        ImGui.SetCursorPos(pos);
        return header;
    }

    private void DrawCache(ObjectCache cache, string id, StringId name, bool condition)
    {
        if (!condition)
            return;

        cache.Update(_now);
        if (cache.Objects.Count == 0)
            return;

        ImGui.PushID(id);
        var header = DrawHeader(name.Value(), cache.GlobalColor.Value(), cache.GlobalTime);
        if (header)
        {
            foreach (var item in cache.Objects)
                Draw(item);
        }
        ImGui.PopID();
    }

    private void DrawRetainers()
        => DrawCache(_retainerCache, "Retainers", StringId.Retainers, _manager.RetainerTimers != null);

    private void DrawMachines()
        => DrawCache(_machineCache, "Machines", StringId.Machines, _manager.MachineTimers != null);

    private void DrawCrops()
    {
        if (_manager.CropTimers == null)
            return;

        _cropCache.Update(_now);
        if (_cropCache.Objects.Count == 0)
            return;

        ImGui.PushID("Crops");
        var header = DrawHeader("Crops", _cropCache.GlobalColor.Value(), _cropCache.GlobalTime);
        if (header)
        {
            foreach (var item in _cropCache.Objects)
                Draw(item);
        }

        ImGui.PopID();
    }
}
