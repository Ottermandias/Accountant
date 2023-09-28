﻿using System;
using Accountant.Internal;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant.SeFunctions;

public sealed class StaticMapContainer : SeFunctionBase<Delegate>
{
    public int MapUiStateOffset;

    public unsafe StaticMapContainer(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, Signatures.MapContainer)
    {
        MapUiStateOffset = Address == IntPtr.Zero ? 0 : *(int*)(Address + 7);
        log.Information($"Next Map Allowance UiState-Offset at +0x{MapUiStateOffset:X}");
    }

    public unsafe DateTime GetMapDateTime(IntPtr uiState)
    {
        if (uiState == IntPtr.Zero || MapUiStateOffset == 0)
            return DateTime.MaxValue;

        var ptr       = uiState + MapUiStateOffset;
        var timestamp = *(ulong*)ptr;
        if (timestamp == ulong.MaxValue)
            return DateTime.MaxValue;

        var time = Helpers.DateFromTimeStamp((uint)timestamp);
        if (time < DateTime.UtcNow.AddSeconds(10))
            return DateTime.MaxValue;

        return time;
    }
}