using System;
using Dalamud.Game;

namespace Accountant.SeFunctions;

public sealed class StaticLeveAllowances : SeAddressBase
{
    public StaticLeveAllowances(SigScanner sigScanner)
        : base(sigScanner, "88 05 ?? ?? ?? ?? 0F B7 41 06")
    { }

    public unsafe int Leves()
    {
        if (Address != IntPtr.Zero)
            return *(byte*)Address;

        return -1;
    }
}
