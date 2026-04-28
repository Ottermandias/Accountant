using System;
using AddonWatcher.Internal;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AddonWatcher.Structs;

public unsafe struct SelectStringInfo
{
    public const int DescriptionNodeIdx = 3;

    public AddonSelectString* Pointer;

    public static implicit operator SelectStringInfo(IntPtr ptr)
        => new() { Pointer = (AddonSelectString*)ptr };

    public AtkComponentList* List
        => Pointer->PopupMenu.PopupMenu.List;

    public int Count
        => List->ListLength;

    public SeString ItemText(int idx)
        => Pointer->AtkValuesSpan[7 + idx].String.AsDalamudSeString();

    public SeString Description
    {
        get
        {
            var count = Pointer->AtkUnitBase.UldManager.NodeListCount;
            if (DescriptionNodeIdx >= count)
                return SeString.Empty;

            var node = Pointer->AtkUnitBase.UldManager.NodeList[DescriptionNodeIdx];
            if (node == null)
                return SeString.Empty;

            return Helpers.TextNodeToString((AtkTextNode*)node);
        }
    }
}
