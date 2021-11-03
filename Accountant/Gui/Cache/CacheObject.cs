using System;
using System.Numerics;
using ImGuiNET;
using ImGuiScene;

namespace Accountant.Gui;

public struct CacheObject
{
    public string        Name;
    public DateTime      DisplayTime;
    public string?       DisplayString;
    public TextureWrap   Icon;
    public float         IconOffset;
    public ColorId       Color;
    public CacheObject[] Children;
    public Action?       TooltipCallback;

    public void DrawIcon()
    {
        if (Icon.Height == 0)
            return;

        if (IconOffset == 0)
        {
            ImGui.Image(Icon.ImGuiHandle, Vector2.One * ImGui.GetTextLineHeight());
        }
        else
        {
            var offset = Vector2.One * IconOffset;
            var size   = Vector2.One - offset;
            ImGui.Image(Icon.ImGuiHandle, Vector2.One * ImGui.GetTextLineHeight(), offset, size);
        }
        ImGui.SameLine();
    }

    public void DrawTooltip()
        => TooltipCallback?.Invoke();
}
