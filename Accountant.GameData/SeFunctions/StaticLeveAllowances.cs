using System;
using Dalamud.Game;

namespace Accountant.SeFunctions;

public sealed class StaticLeveAllowances : SeAddressBase
{
    public StaticLeveAllowances(SigScanner sigScanner)
        : base(sigScanner, Signatures.LeveAllowances)
    { }

    public unsafe int Leves()
    {
        if (Address != IntPtr.Zero)
            return *(byte*)Address;

        return -1;
    }
}
