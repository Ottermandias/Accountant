using System;
using AddonWatcher.Internal;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AddonWatcher.Structs;

public unsafe struct TalkInfo
{
    public AddonTalk* Pointer;

    public static implicit operator TalkInfo(IntPtr ptr)
        => new() { Pointer = (AddonTalk*)ptr };

    public bool IsVisible
        => Pointer->AtkUnitBase.IsVisible;

    public SeString Speaker
        => Helpers.TextNodeToString(Pointer->AtkTextNode220);

    public SeString Text
        => Helpers.TextNodeToString(Pointer->AtkTextNode228);
}
