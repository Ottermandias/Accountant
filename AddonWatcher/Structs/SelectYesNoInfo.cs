using System;
using AddonWatcher.Internal;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AddonWatcher.Structs;

public unsafe struct SelectYesNoInfo
{
    public const int YesButtonId       = 0;
    public const int NoButtonId        = 1;

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
            var node = Pointer->PromptText;
            return node == null ? SeString.Empty : Helpers.TextNodeToString(node);
        }
    }
}
