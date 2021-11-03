using System;
using System.Collections.Generic;
using System.Numerics;
using Accountant.Enums;
using Accountant.Gui.Helper;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;

namespace Accountant.Gui;

public partial class TimerWindow
{
//        private StateInfo    _allMachines = StateInfo.Empty;



//
//
//        private void DrawMachineRow(string name, DateTime time, MachineType type)
//        {
//            ImGui.TableNextRow();
//            ImGui.TableNextColumn();
//            var handle = type == MachineType.Submarine ? _submarineIcon?.ImGuiHandle : _airshipIcon?.ImGuiHandle;
//            if (handle != null)
//            {
//                ImGui.Image(handle.Value, Vector2.One * ImGui.GetTextLineHeight());
//                ImGui.SameLine();
//            }
//
//            ImGui.Selectable(name);
//            ImGui.TableNextColumn();
//            ImGui.Text(ConvertDateTime(_now, time));
//        }
//
//        private void DrawMachines()
//        {
//            _allMachines = StateInfo.Empty;
//            string? removeFc = null;
//            foreach (var (fc, machines) in Peon.Timers.Machines)
//            {
//                var info = new StateInfo(_now, machines.Select(m => m.Value.Item1), true);
//                _allMachines = StateInfo.Combine(_allMachines, info);
//                if (_drawData)
//                {
//                    var collapse = ColorHeader(fc, info);
//                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
//                        removeFc = fc;
//                    if (!collapse)
//                        continue;
//
//                    using var table = SetupTable($"##Machines_{fc}", _widthTime + 15);
//                    if (!table)
//                        continue;
//
//                    foreach (var (name, (time, type)) in machines)
//                        DrawMachineRow(name, time, type);
//                }
//            }
//
//            if (removeFc != null && Peon.Timers.Machines.Remove(removeFc))
//                Peon.Timers.SaveMachines();
//        }
}
