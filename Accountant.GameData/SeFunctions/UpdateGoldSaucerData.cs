using System;
using Dalamud.Game;

namespace Accountant.SeFunctions;


public delegate void UpdateGoldSaucerDelegate(IntPtr unk, IntPtr packetData);

public sealed class UpdateGoldSaucerData : SeFunctionBase<UpdateGoldSaucerDelegate>
{
    public UpdateGoldSaucerData(SigScanner sigScanner)
        : base(sigScanner, Signatures.GoldSaucerData)
    { }
}
