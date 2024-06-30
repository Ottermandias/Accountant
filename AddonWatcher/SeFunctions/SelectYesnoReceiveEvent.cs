using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

public sealed class SelectYesnoReceiveEvent : SeFunctionBase<OnAddonReceiveEventDelegate>
{
    public SelectYesnoReceiveEvent(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner,
            "40 53 55 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 49 8B D9")
    { }
}
