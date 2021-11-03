using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace Accountant.Gui.Helper;

public static partial class ImGuiRaii
{
    public static void HoverTooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    public static bool Checkmark(string label, bool current, Action<bool> setter)
    {
        var tmp = current;
        if (ImGui.Checkbox(label, ref tmp) && tmp != current)
        {
            setter(tmp);
            return true;
        }

        return false;
    }

    public static void ConfigCheckmark(string label, bool current, Action<bool> setter)
    {
        if (Checkmark(label, current, setter))
            Accountant.Config.Save();
    }

    public static bool ColorPicker(string label, string tooltip, uint current, Action<uint> setter, uint standard)
    {
        var ret = false;
        var old = ImGui.ColorConvertU32ToFloat4(current);
        var tmp = old;
        ImGui.BeginGroup();
        using var alpha = PushStyle(ImGuiStyleVar.Alpha, 0.5f, current == standard);
        if (ImGui.Button($"Default##{label}") && current != standard)
        {
            setter(standard);
            ret = true;
        }
        alpha.Pop();

        ImGui.SameLine();
        if (ImGui.ColorEdit4(label, ref tmp, ImGuiColorEditFlags.AlphaPreviewHalf | ImGuiColorEditFlags.NoInputs) && tmp != old)
        {
            setter(ImGui.ColorConvertFloat4ToU32(tmp));
            ret = true;
        }

        ImGui.EndGroup();
        if (tooltip.Any())
        {
            HoverTooltip(tooltip);
        }


        return ret;
    }

    public static void ConfigColorPicker(string label, string tooltip, uint current, Action<uint> setter, uint standard)
    {
        if (ColorPicker(label, tooltip, current, setter, standard))
            Accountant.Config.Save();
    }
}

public static partial class ImGuiRaii
{
    public static Style PushStyle(ImGuiStyleVar idx, float value, bool condition = true)
        => new Style().Push(idx, value, condition);

    public static Style PushStyle(ImGuiStyleVar idx, Vector2 value, bool condition = true)
        => new Style().Push(idx, value, condition);

    public class Style : IDisposable
    {
        private int _count;

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CheckStyleIdx(ImGuiStyleVar idx, Type type)
        {
            var shouldThrow = idx switch
            {
                ImGuiStyleVar.Alpha               => type != typeof(float),
                ImGuiStyleVar.WindowPadding       => type != typeof(Vector2),
                ImGuiStyleVar.WindowRounding      => type != typeof(float),
                ImGuiStyleVar.WindowBorderSize    => type != typeof(float),
                ImGuiStyleVar.WindowMinSize       => type != typeof(Vector2),
                ImGuiStyleVar.WindowTitleAlign    => type != typeof(Vector2),
                ImGuiStyleVar.ChildRounding       => type != typeof(float),
                ImGuiStyleVar.ChildBorderSize     => type != typeof(float),
                ImGuiStyleVar.PopupRounding       => type != typeof(float),
                ImGuiStyleVar.PopupBorderSize     => type != typeof(float),
                ImGuiStyleVar.FramePadding        => type != typeof(Vector2),
                ImGuiStyleVar.FrameRounding       => type != typeof(float),
                ImGuiStyleVar.FrameBorderSize     => type != typeof(float),
                ImGuiStyleVar.ItemSpacing         => type != typeof(Vector2),
                ImGuiStyleVar.ItemInnerSpacing    => type != typeof(Vector2),
                ImGuiStyleVar.IndentSpacing       => type != typeof(float),
                ImGuiStyleVar.CellPadding         => type != typeof(Vector2),
                ImGuiStyleVar.ScrollbarSize       => type != typeof(float),
                ImGuiStyleVar.ScrollbarRounding   => type != typeof(float),
                ImGuiStyleVar.GrabMinSize         => type != typeof(float),
                ImGuiStyleVar.GrabRounding        => type != typeof(float),
                ImGuiStyleVar.TabRounding         => type != typeof(float),
                ImGuiStyleVar.ButtonTextAlign     => type != typeof(Vector2),
                ImGuiStyleVar.SelectableTextAlign => type != typeof(Vector2),
                _                                 => throw new ArgumentOutOfRangeException(nameof(idx), idx, null),
            };

            if (shouldThrow)
                throw new ArgumentException($"Unable to push {type} to {idx}.");
        }

        public Style Push(ImGuiStyleVar idx, float value, bool condition = true)
        {
            if (condition)
            {
                CheckStyleIdx(idx, typeof(float));
                ImGui.PushStyleVar(idx, value);
                ++_count;
            }

            return this;
        }

        public Style Push(ImGuiStyleVar idx, Vector2 value, bool condition = true)
        {
            if (condition)
            {
                CheckStyleIdx(idx, typeof(Vector2));
                ImGui.PushStyleVar(idx, value);
                ++_count;
            }

            return this;
        }

        public void Pop(int num = 1)
        {
            num    =  Math.Min(num, _count);
            _count -= num;
            ImGui.PopStyleVar(num);
        }

        public void Dispose()
            => Pop(_count);
    }
}
