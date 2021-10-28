using System;
using System.Linq;
using AddonWatcher.Enums;
using AddonWatcher.Structs;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AddonWatcher.Internal;

internal partial class AddonWatcherBase
{
    private void SelectYesnoOnSetupDetour(IntPtr unit, int _, IntPtr data)
    {
        SelectYesnoSetupHook!.Original(unit, _, data);
        var description = Dalamud.Memory.MemoryHelper.ReadSeStringNullTerminated(data + 0x08);
        var yes         = Dalamud.Memory.MemoryHelper.ReadSeStringNullTerminated(data + 0x18);
        var no          = Dalamud.Memory.MemoryHelper.ReadSeStringNullTerminated(data + 0x28);
        SelectYesnoSetup?.Invoke(unit, description, yes, no);
    }

    private void SelectStringOnSetupDetour(IntPtr unit, int _, IntPtr data)
    {
        SelectStringSetupHook!.Original(unit, _, data);
        var ptr         = (SelectStringInfo)unit;
        var description = ptr.Description;
        var options     = Enumerable.Range(0, ptr.Count).Select(i => ptr.ItemText(i)).ToArray();
        SelectStringSetup?.Invoke(unit, description, options);
    }

    private void JournalResultOnSetupDetour(IntPtr unit, int _, IntPtr data)
    {
        JournalResultSetupHook!.Original(unit, _, data);
        var ptr       = (JournalResultInfo)unit;
        var questName = ptr.QuestName;
        JournalResultSetup?.Invoke(unit, questName);
    }

    private void SelectYesNoEventDetour(IntPtr atkUnit, EventType eventType, int which, IntPtr source, IntPtr data)
    {
        if (eventType == EventType.Change)
        {
            var ptr             = (SelectYesNoInfo)atkUnit;
            var descriptionText = ptr.Description;
            switch (which)
            {
                case SelectYesNoInfo.YesButtonId:
                    var yesText = ptr.YesText;
                    YesnoSelected!.Invoke(atkUnit, true, yesText, descriptionText);
                    break;
                case SelectYesNoInfo.NoButtonId:
                    var noText = ptr.NoText;
                    YesnoSelected!.Invoke(atkUnit, false, noText, descriptionText);
                    break;
            }
        }

        SelectYesNoHook!.Original(atkUnit, eventType, which, source, data);
    }

    private unsafe void SelectStringEventDetour(IntPtr atkUnit, EventType eventType, int which, IntPtr source, IntPtr data)
    {
        if (eventType == EventType.ListIndexChange && data != IntPtr.Zero)
        {
            var ptr             = (SelectStringInfo)atkUnit;
            var idx             = ((byte*)data)[0x10];
            var renderer        = *(AtkComponentListItemRenderer**)data;
            var descriptionText = ptr.Description;
            var itemText        = Helpers.TextNodeToString(renderer->AtkComponentButton.ButtonTextNode);
            StringSelected!.Invoke(atkUnit, idx, itemText, descriptionText);
        }

        SelectStringHook!.Original(atkUnit, eventType, which, source, data);
    }


    private void TalkUpdateDetour(IntPtr unit, IntPtr data)
    {
        TalkUpdateHook!.Original(unit, data);
        var ptr     = (TalkInfo)unit;
        var speaker = ptr.Speaker;
        var text    = ptr.Text;
        TalkOnUpdate?.Invoke(unit, speaker, text);
    }
}
