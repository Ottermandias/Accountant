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
            "?? 89 ?? ?? ?? 57 48 83 ?? ?? 0F B7 ?? 49 8B ?? 48 8B ?? 83 F8 ?? 0F ?? ?? ?? ?? ?? 83 F8 ?? 0F ?? ?? ?? ?? ?? 83 F8 ?? 74 ?? 83 F8 ?? 0F ?? ?? ?? ?? ?? 48 ?? ?? ?? ?? 48")
    { }
}
