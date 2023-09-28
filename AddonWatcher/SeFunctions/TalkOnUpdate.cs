using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

// Function VTable[47] for everything inheriting from AtkUnitBase.
public delegate void OnAddonUpdateDelegate(IntPtr a, IntPtr b);

public sealed class TalkOnUpdate : SeFunctionBase<OnAddonUpdateDelegate>
{
    public TalkOnUpdate(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, "40 ?? 57 48 83 ?? 58 48 ?? ?? 18 01 ?? ?? 48 8B ??")
    { }
}
