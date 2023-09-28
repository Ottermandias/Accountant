using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

// Function VTable[44] for everything inheriting from AtkUnitBase.
public delegate void OnAddonSetupDelegate(IntPtr a, int b, IntPtr c);

public sealed class SelectYesnoOnSetup : SeFunctionBase<OnAddonSetupDelegate>
{
    public SelectYesnoOnSetup(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, "?? 89 ?? ?? ?? ?? 89 ?? ?? ?? ?? 89 ?? ?? ?? 57 41 ?? 41 ?? 48 83 ?? 40 44 8B ?? ?? 29 ?? ?? ?? BA 02")
    { }
}
