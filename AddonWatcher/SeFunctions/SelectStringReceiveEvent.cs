using System;
using AddonWatcher.Enums;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

public delegate void OnAddonReceiveEventDelegate(IntPtr atkUnit, EventType eventType, int which, IntPtr source, IntPtr data);

public sealed class SelectStringReceiveEvent : SeFunctionBase<OnAddonReceiveEventDelegate>
{
    public SelectStringReceiveEvent(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner,
            "48 89 5C 24 ?? 57 48 83 EC ?? 49 8B F9 48 8B D9 66 83 FA ?? 0F 84 ?? ?? ?? ?? 66 83 FA ?? 0F 84 ?? ?? ?? ?? 66 83 FA ?? 74 ?? 66 83 FA ?? 0F 85 ?? ?? ?? ?? 48 8B 5C 24")
    { }
}
