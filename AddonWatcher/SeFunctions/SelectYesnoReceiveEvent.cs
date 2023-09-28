using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

public sealed class SelectYesnoReceiveEvent : SeFunctionBase<OnAddonReceiveEventDelegate>
{
    public SelectYesnoReceiveEvent(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner,
            "40 ?? 55 57 48 81 ?? ?? ?? ?? ?? 48 8B ?? ?? ?? ?? ?? 48 33 ?? ?? 89 ?? ?? ?? 0F B7")
    { }
}
