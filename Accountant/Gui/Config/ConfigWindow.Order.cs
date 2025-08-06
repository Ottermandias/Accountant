using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Gui.Helper;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace Accountant.Gui.Config;

public partial class ConfigWindow
{
    private readonly List<(int, string)> _priorityCache = new(Accountant.Config.Priorities.Count);
    private          int                 _newPriority;
    private          string              _newPriorityName = string.Empty;
    private          int                 _dragDropSource  = -1;

    private void BuildCache()
    {
        if (_priorityCache.Count > 0)
            return;

        foreach (var (name, priority) in Accountant.Config.Priorities.OrderByDescending(kvp => kvp.Value))
            _priorityCache.Add((priority, name));
    }

    private static unsafe bool IsDropping(string name)
        => ImGui.AcceptDragDropPayload(name).Handle != null;

    private void DrawOrderNamesTab()
    {
        if (!ImGui.BeginTabItem("Order##AccountantTabs"))
            return;

        using var raii = ImGuiRaii.DeferredEnd(ImGui.EndTabItem);

        if (!ImGui.BeginChild("##OrderTab"))
            return;

        raii.Push(ImGui.EndChild);

        using var ids = ImGuiRaii.PushId("Order");

        if (!ImGui.BeginTable(string.Empty, 3))
            return;

        raii.Push(ImGui.EndTable);

        ImGui.TableSetupColumn("##",       ImGuiTableColumnFlags.WidthFixed, ImGui.GetStyle().FrameBorderSize);
        ImGui.TableSetupColumn("Priority", ImGuiTableColumnFlags.WidthFixed, 150 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Name",     ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        ImGui.TableSetupScrollFreeze(0, 1);

        BuildCache();

        int? change      = null;
        var  newPriority = int.MinValue;
        for (var i = 0; i < _priorityCache.Count; ++i)
        {
            var (priority, name) = _priorityCache[i];
            ids.Push(i);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            using var font = ImGuiRaii.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                change = i;
            font.Pop();
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputInt(string.Empty, ref priority, 0, 0, flags: ImGuiInputTextFlags.EnterReturnsTrue) && priority != _priorityCache[i].Item1)
            {
                change      = i;
                newPriority = priority;
            }

            ImGui.TableNextColumn();
            ImGui.Selectable(name);

            if (ImGui.BeginDragDropSource())
            {
                ImGui.SetDragDropPayload("Priority", [], ImGuiCond.None);
                _dragDropSource = i;
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget() && _dragDropSource >= 0 && IsDropping("Priority"))
            {
                if (i < _dragDropSource)
                {
                    change = _dragDropSource;
                    if (i == 0)
                        newPriority = priority + 100;
                    else
                        newPriority = (priority + _priorityCache[i - 1].Item1) / 2;
                }
                else if (i > _dragDropSource)
                {
                    change = _dragDropSource;
                    if (i == _priorityCache.Count - 1)
                        newPriority = priority - 100;
                    else
                        newPriority = (priority + _priorityCache[i + 1].Item1) / 2;
                }

                ImGui.EndDragDropTarget();
                _dragDropSource = -1;
            }

            ids.Pop();
        }

        if (change != null)
        {
            if (newPriority == int.MinValue)
                Accountant.Config.Priorities.Remove(_priorityCache[change.Value].Item2);
            else
                Accountant.Config.Priorities[_priorityCache[change.Value].Item2] = newPriority;

            Accountant.Config.Save();
            _timerWindow.ResetCache();
            _priorityCache.Clear();
        }

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var _ = ImGuiRaii.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && _newPriorityName.Any())
            if (Accountant.Config.Priorities.TryAdd(_newPriorityName, _newPriority))
            {
                Accountant.Config.Save();
                _timerWindow.SortCache();
                _timerWindow.ResetCache();
                _priorityCache.Clear();
            }

        _.Pop();

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##newPriority", ref _newPriority, 0, 0);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##name", "New Priority Name...", ref _newPriorityName, 48);
    }
}
