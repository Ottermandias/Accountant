using System;
using Accountant.SeFunctions;
using Accountant.Structs;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant;

public unsafe class RetainerManager
{
    private static StaticRetainerContainer? _address;
    private static RetainerContainer*       _container;

    public RetainerManager(IPluginLog log, ISigScanner sigScanner)
    {
        if (_address != null)
            return;

        _address   ??= new StaticRetainerContainer(log, sigScanner);
        _container =   (RetainerContainer*)_address.Address;
    }

    public bool Ready
        => _container != null && _container->Ready == 1;

    public int Count
        => Ready ? _container->RetainerCount : 0;

    public SeRetainer Retainer(int which)
        => which < Count
            ? ((SeRetainer*)_container->Retainers)[which]
            : throw new ArgumentOutOfRangeException($"Invalid retainer {which} requested, only {Count} available.");
}
