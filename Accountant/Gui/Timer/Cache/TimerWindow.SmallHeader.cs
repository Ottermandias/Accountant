using System;
using System.Numerics;
using Accountant.Gui.Helper;
using ImGuiNET;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    public partial class BaseCache
    {
        public struct SmallHeader
        {
            public ColorId  Color;
            public DateTime DisplayTime;
            public string   Name;
            public int      ObjectsBegin;
            public int      ObjectsCount;

            public void Draw(BaseCache cache, DateTime now)
            {
                if (ObjectsCount == 0 || Accountant.Config.HideDisabled && Color == ColorId.DisabledText)
                    return;

                using var color  = ImGuiRaii.PushColor(ImGuiCol.Text, Color.Value());
                using var indent = ImGuiRaii.PushStyle(ImGuiStyleVar.IndentSpacing, ImGui.GetStyle().IndentSpacing / 2);
                using var id     = ImGuiRaii.PushId(Name);

                var posY   = ImGui.GetCursorPosY();
                var header = ImGui.TreeNodeEx(Name);
                if (DisplayTime > now && DisplayTime != DateTime.MaxValue)
                {
                    var s     = TimeSpanString(DisplayTime - now);
                    var width = ImGui.CalcTextSize(s).X;
                    var pos   = ImGui.GetCursorPos();
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - width, posY));
                    ImGui.Text(s);
                    ImGui.SetCursorPos(pos);
                }

                if (!header)
                    return;

                color.Pop();
                using var _   = ImGuiRaii.DeferredEnd(ImGui.TreePop);
                var       end = ObjectsBegin + ObjectsCount;
                for (var i = ObjectsBegin; i < end; ++i)
                    cache.Objects[i].Draw(now);
            }
        }
    }
}
