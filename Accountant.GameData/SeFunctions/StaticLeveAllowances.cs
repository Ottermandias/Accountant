using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant.SeFunctions;

public sealed class StaticLeveAllowances : SeAddressBase
{
    public StaticLeveAllowances(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, Signatures.LeveAllowances)
    { }

    public unsafe int Leves()
    {
        if (Address != IntPtr.Zero)
            return *(byte*)Address;

        return -1;
    }
}
