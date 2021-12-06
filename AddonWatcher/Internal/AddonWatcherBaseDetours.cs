using System;
using System.Linq;
using AddonWatcher.Enums;
using AddonWatcher.Structs;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
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
        PluginLog.Verbose("SelectYesno 0x{SelectYesnoPtr:X} setup with description {Description} and options {YesOption}/{NoOption}.",
            (ulong)unit, description, yes, no);
        SelectYesnoSetup?.Invoke(unit, description, yes, no);
    }

    private void SelectStringOnSetupDetour(IntPtr unit, int _, IntPtr data)
    {
        SelectStringSetupHook!.Original(unit, _, data);
        var ptr         = (SelectStringInfo)unit;
        var description = ptr.Description;
        var options     = Enumerable.Range(0, ptr.Count).Select(i => ptr.ItemText(i)).ToArray();
        PluginLog.Verbose("SelectString 0x{SelectStringPtr:X} setup with description {Description} and {OptionCount} options.", (ulong)unit,
            description, options.Length);
        SelectStringSetup?.Invoke(unit, description, options);
    }

    private void JournalResultOnSetupDetour(IntPtr unit, int _, IntPtr data)
    {
        JournalResultSetupHook!.Original(unit, _, data);
        var ptr       = (JournalResultInfo)unit;
        var questName = ptr.QuestName;
        PluginLog.Verbose("JournalResult 0x{JournalResultPtr:X} setup for quest {QuestName}.", (ulong)unit, questName);
        JournalResultSetup?.Invoke(unit, questName);
    }

    private void LotteryWeeklyRewardListOnSetupDetour(IntPtr unit, int _, IntPtr data)
    {
        LotteryWeeklyRewardListSetupHook!.Original(unit, _, data);
        PluginLog.Verbose("LotteryWeeklyRewardList 0x{LotteryWeeklyRewardListPtr:X} setup.", (ulong)unit);
        LotteryWeeklyRewardListSetup?.Invoke(unit);
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
                    PluginLog.Verbose("Yes-Button {ButtonText} selected on 0x{SelectYesnoPtr:X} with description {Description}.", yesText,
                        (ulong)atkUnit, descriptionText);
                    YesnoSelected!.Invoke(atkUnit, true, yesText, descriptionText);
                    break;
                case SelectYesNoInfo.NoButtonId:
                    var noText = ptr.NoText;
                    PluginLog.Verbose("No-Button {ButtonText} selected on 0x{SelectYesnoPtr:X} with description {Description}.", noText,
                        (ulong)atkUnit, descriptionText);
                    YesnoSelected!.Invoke(atkUnit, false, noText, descriptionText);
                    break;
            }
        }

        SelectYesNoHook!.Original(atkUnit, eventType, which, source, data);
    }

    private static readonly ByteString SelectStringName = new("SelectString");

    private unsafe void SelectStringEventDetour(IntPtr atkUnit, EventType eventType, int which, IntPtr source, IntPtr data)
    {
        if (eventType == EventType.ListIndexChange && data != IntPtr.Zero)
        {
            var owner = ((PopupMenu*)atkUnit)->Owner;
            if (SelectStringName.Equals(owner->Name))
            {
                var ptr             = (IntPtr)owner;
                var idx             = ((byte*)data)[0x10];
                var renderer        = *(AtkComponentListItemRenderer**)data;
                var descriptionText = ((SelectStringInfo)ptr).Description;
                var itemText        = Helpers.TextNodeToString(renderer->AtkComponentButton.ButtonTextNode);
                PluginLog.Verbose("String {ButtonText} ({Which}) selected on 0x{SelectStringPtr:X} with description {Description}.", itemText,
                    which, (ulong)ptr, descriptionText);
                StringSelected!.Invoke(ptr, idx, itemText, descriptionText);
            }
        }

        SelectStringHook!.Original(atkUnit, eventType, which, source, data);
    }


    private void TalkUpdateDetour(IntPtr unit, IntPtr data)
    {
        TalkUpdateHook!.Original(unit, data);
        var ptr     = (TalkInfo)unit;
        var speaker = ptr.Speaker;
        var text    = ptr.Text;
        PluginLog.Verbose("Talk at 0x{Unit:X} updated - Speaker: {Speaker}\n{Text}", (ulong)unit, speaker, text);
        TalkUpdated?.Invoke(unit, text, speaker);
    }
}
