using Dalamud.Game;

namespace AddonWatcher.SeFunctions;

public sealed class SelectStringOnSetup : SeFunctionBase<OnAddonSetupDelegate>
{
    public SelectStringOnSetup(SigScanner sigScanner)
        : base(sigScanner, "40 ?? 56 57 41 ?? 41 ?? 41 ?? 48 83 ?? ?? 4D")
    { }
}
