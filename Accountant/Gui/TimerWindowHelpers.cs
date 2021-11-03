using System;
using System.Collections.Generic;
using Dalamud.Interface;
using ImGuiNET;

namespace Accountant.Gui;

public partial class TimerWindow
{
//       private struct StateInfo
//       {
//           public TimeSpan FirstObjectAvailable;
//           public TimeSpan AllObjectsAvailable;
//           public int      AvailableObjects;
//           public int      SentObjects;
//           public int      FinishedObjects;
//
//           public uint Color()
//               => AllObjectsAvailable == TimeSpan.Zero 
//                   ? Accountant.Config.GreenHeader 
//                   : FirstObjectAvailable == TimeSpan.Zero 
//                       ? Accountant.Config.YellowHeader 
//                       : Accountant.Config.RedHeader;
//
//           public StateInfo(DateTime now, IEnumerable<DateTime> times, bool machine)
//           {
//               AvailableObjects     = 0;
//               SentObjects          = 0;
//               FinishedObjects      = 0;
//               FirstObjectAvailable = TimeSpan.MaxValue;
//               AllObjectsAvailable  = TimeSpan.Zero;
//               foreach (var time in times)
//                   if (time == DateTime.MinValue)
//                   {
//                       ++AvailableObjects;
//                       if (!machine)
//                           FirstObjectAvailable = TimeSpan.Zero;
//                   }
//                   else if (time <= now)
//                   {
//                       ++FinishedObjects;
//                       FirstObjectAvailable = TimeSpan.Zero;
//                   }
//                   else
//                   {
//                       ++SentObjects;
//                       var tmp = time - now;
//                       if (tmp > AllObjectsAvailable)
//                           AllObjectsAvailable = tmp;
//                       if (tmp < FirstObjectAvailable)
//                           FirstObjectAvailable = tmp;
//                   }
//
//               if (SentObjects < 4 && AvailableObjects > 0)
//                   FirstObjectAvailable = TimeSpan.Zero;
//           }
//
//           public static readonly StateInfo Empty = new(DateTime.MinValue, Array.Empty<DateTime>(), false);
//
//           public static StateInfo Combine(StateInfo lhs, StateInfo rhs)
//               => new()
//               {
//                   FirstObjectAvailable =
//                       rhs.FirstObjectAvailable < lhs.FirstObjectAvailable ? rhs.FirstObjectAvailable : lhs.FirstObjectAvailable,
//                   AllObjectsAvailable =
//                       rhs.AllObjectsAvailable > lhs.AllObjectsAvailable ? rhs.AllObjectsAvailable : lhs.AllObjectsAvailable,
//                   AvailableObjects = rhs.AvailableObjects + lhs.AvailableObjects,
//                   SentObjects      = rhs.SentObjects + lhs.SentObjects,
//                   FinishedObjects  = rhs.FinishedObjects + lhs.FinishedObjects,
//               };
//       }
//
    private static string TimeSpanString(TimeSpan span, int align = 2)
        => $"{((int)span.TotalHours).ToString(align == 2 ? "D2" : "D3")}:{span.Minutes:D2}:{span.Seconds:D2}";
//
//       private bool ColorHeader(string header, StateInfo infos)
//       {
//           var color = infos.Color();
//           ImGui.PushStyleColor(ImGuiCol.Header, color);
//           var text = string.Empty;
//           if (color == Accountant.Config.YellowHeader)
//               text = TimeSpanString(infos.AllObjectsAvailable);
//           else if (color == Accountant.Config.RedHeader)
//               text = TimeSpanString(infos.FirstObjectAvailable);
//
//           ImGui.BeginGroup();
//           var collapse = ImGui.CollapsingHeader(header);
//           ImGui.SameLine((_widthTotal - _widthShortTime) * ImGuiHelpers.GlobalScale - 2 * ImGui.GetStyle().ItemSpacing.X - ImGui.GetStyle().ScrollbarSize);
//           ImGui.Text(text);
//           ImGui.EndGroup();
//           ImGui.PopStyleColor();
//           if (ImGui.IsItemHovered())
//               ImGui.SetTooltip("Right Click to clear.");
//           return collapse;
//       }
//
//       private  SetupTable(string label, float width)
//       {
//           var table = new ImGuiRaii();
//           if (!table.Begin(() => ImGui.BeginTable(label, 2), ImGui.EndTable))
//               return table;
//
//           ImGui.TableSetupColumn("0", ImGuiTableColumnFlags.None, (_widthTotal - width) * ImGuiHelpers.GlobalScale);
//           ImGui.TableSetupColumn("1", ImGuiTableColumnFlags.None, width * ImGuiHelpers.GlobalScale);
//           return table;
//       }
//
//       private static string ConvertDateTime(DateTime now, DateTime time)
//       {
//           if (time == DateTime.MinValue)
//               return "Not Sent";
//           if (time <= now)
//               return "Completed";
//
//           return TimeSpanString(time - now);
//       }
}
