using System;
using AddonWatcher.Internal;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AddonWatcher.Structs;

public unsafe struct JournalResultInfo
{
    public const int CompleteButtonId = 1;
    public const int QuestNameNodeIdx = 11;

    public AddonJournalResult* Pointer;

    public static implicit operator JournalResultInfo(IntPtr ptr)
        => new() { Pointer = (AddonJournalResult*)ptr };

    public SeString QuestName
        => Helpers.TextNodeToString((AtkTextNode*)Pointer->AtkUnitBase.UldManager.NodeList[QuestNameNodeIdx]);
}
