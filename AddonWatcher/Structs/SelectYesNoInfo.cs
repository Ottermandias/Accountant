using System;
using AddonWatcher.Internal;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AddonWatcher.Structs;

public unsafe struct SelectYesNoInfo
{
    public const int YesButtonId       = 0;
    public const int NoButtonId        = 1;
    public const int CheckMarkId       = 3;
    public const int DescriptionNodeId = 2;

    public AddonSelectYesno* Pointer;

    public static implicit operator SelectYesNoInfo(IntPtr ptr)
        => new() { Pointer = (AddonSelectYesno*)ptr };

    public SeString YesText
        => Helpers.TextNodeToString(Pointer->YesButton->ButtonTextNode);

    public SeString NoText
        => Helpers.TextNodeToString(Pointer->NoButton->ButtonTextNode);

    public SeString Description
    {
        get
        {
            // TODO: use clientstructs Prompt member when its merged
            var node = *(AtkTextNode**)((byte*)Pointer + 0x220);
            return node == null ? SeString.Empty : Helpers.TextNodeToString(node);
        }
    }
}
