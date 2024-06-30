using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

public sealed class JournalResultOnSetup : SeFunctionBase<OnAddonSetupDelegate>
{
    public JournalResultOnSetup(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner,
            "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 8B EA 49 8B F0 BA ?? ?? ?? ?? 48 8B F9 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 89 87")
    { }
}
