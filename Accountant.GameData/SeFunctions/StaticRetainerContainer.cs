using Dalamud.Game;

namespace Accountant.SeFunctions;

public sealed class StaticRetainerContainer : SeAddressBase
{
    public StaticRetainerContainer(SigScanner sigScanner)
        : base(sigScanner, Signatures.RetainerContainer)
    { }
}