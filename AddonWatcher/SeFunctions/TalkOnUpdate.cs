using System;
using Dalamud.Game;

namespace AddonWatcher.SeFunctions;

// Function VTable[47] for everything inheriting from AtkUnitBase.
public delegate void OnAddonUpdateDelegate(IntPtr a, IntPtr b);

public sealed class TalkOnUpdate : SeFunctionBase<OnAddonUpdateDelegate>
{
    public TalkOnUpdate(SigScanner sigScanner)
        : base(sigScanner, "40 ?? 57 48 83 ?? 58 48 ?? ?? 08 01 ?? ?? 48 8B ??")
    { }
}
