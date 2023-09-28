using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

public sealed class SelectStringOnSetup : SeFunctionBase<OnAddonSetupDelegate>
{
    public SelectStringOnSetup(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, "40 ?? 56 57 41 ?? 41 ?? 41 ?? 48 83 ?? ?? 4D")
    { }
}
