using Dalamud.Game;

namespace AddonWatcher.SeFunctions;

public sealed class SelectYesnoReceiveEvent : SeFunctionBase<OnAddonReceiveEventDelegate>
{
    public SelectYesnoReceiveEvent(SigScanner sigScanner)
        : base(sigScanner,
            "40 ?? 55 57 48 81 ?? ?? ?? ?? ?? 48 8B ?? ?? ?? ?? ?? 48 33 ?? ?? 89 ?? ?? ?? 0F B7")
    { }
}
