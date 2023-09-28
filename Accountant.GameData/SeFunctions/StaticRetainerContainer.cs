using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant.SeFunctions;

public sealed class StaticRetainerContainer : SeAddressBase
{
    public StaticRetainerContainer(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, Signatures.RetainerContainer)
    { }
}