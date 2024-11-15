using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant.SeFunctions;

public delegate void UpdateGoldSaucerDelegate(IntPtr unk, IntPtr packetData);

public sealed class UpdateGoldSaucerData(IPluginLog log, ISigScanner sigScanner)
    : SeFunctionBase<UpdateGoldSaucerDelegate>(log, sigScanner, Signatures.GoldSaucerData);
