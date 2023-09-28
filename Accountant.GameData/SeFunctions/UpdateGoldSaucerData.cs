using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant.SeFunctions;

public delegate void UpdateGoldSaucerDelegate(IntPtr unk, IntPtr packetData);

public sealed class UpdateGoldSaucerData : SeFunctionBase<UpdateGoldSaucerDelegate>
{
    public UpdateGoldSaucerData(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, Signatures.GoldSaucerData)
    { }
}
