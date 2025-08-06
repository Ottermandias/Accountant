using System;
using System.Linq;
using Dalamud.Bindings.ImGui;

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
        if (!ImGui.Checkbox(label, ref tmp) || tmp == current)
            return false;

        setter(tmp);
        return true;
    }

    public static void ConfigCheckmark(string label, bool current, Action<bool> setter)
    {
        if (Checkmark(label, current, setter))
            Accountant.Config.Save();
    }

    private static string ColorBytes(uint color)
        => $"#{(byte)(color & 0xFF):X2}{(byte)(color >> 8):X2}{(byte)(color >> 16):X2}{(byte)(color >> 24):X2}";

    public static bool ColorPicker(string label, string tooltip, uint current, Action<uint> setter, uint standard)
    {
        var       ret = false;
        var       old = ImGui.ColorConvertU32ToFloat4(current);
        var       tmp = old;
        using var _   = PushId(label);
        ImGui.BeginGroup();
        if (ImGui.ColorEdit4("", ref tmp, ImGuiColorEditFlags.AlphaPreviewHalf | ImGuiColorEditFlags.NoInputs) && tmp != old)
        {
            setter(ImGui.ColorConvertFloat4ToU32(tmp));
            ret = true;
        }

        ImGui.SameLine();
        using var alpha = PushStyle(ImGuiStyleVar.Alpha, 0.5f, current == standard);
        if (ImGui.Button("Default") && current != standard)
        {
            setter(standard);
            ret = true;
        }

        alpha.Pop();
        HoverTooltip($"Reset this color to {ColorBytes(standard)}.");

        ImGui.SameLine();
        ImGui.Text(label);
        if (tooltip.Any())
            HoverTooltip(tooltip);
        ImGui.EndGroup();

        return ret;
    }

    public static void ConfigColorPicker(string label, string tooltip, uint current, Action<uint> setter, uint standard)
    {
        if (ColorPicker(label, tooltip, current, setter, standard))
            Accountant.Config.Save();
    }
}
