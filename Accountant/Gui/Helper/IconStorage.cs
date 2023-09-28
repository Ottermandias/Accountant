using System;
using System.Collections.Generic;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

namespace Accountant.Gui.Helper;

internal class IconStorage : IDisposable
{
    private readonly ITextureProvider                      _provider;
    private readonly Dictionary<uint, IDalamudTextureWrap> _icons;

    public IconStorage(ITextureProvider provider, int size = 0)
    {
        _provider = provider;
        _icons    = new Dictionary<uint, IDalamudTextureWrap>(size);
    }

    public IDalamudTextureWrap this[uint id]
        => LoadIcon(id);

    public IDalamudTextureWrap this[int id]
        => LoadIcon((uint)id);

    public IDalamudTextureWrap LoadIcon(uint id)
    {
        if (_icons.TryGetValue(id, out var ret))
            return ret;

        ret        = _provider.GetIcon(id)!;
        _icons[id] = ret;
        return ret;
    }

    public void Dispose()
    {
        foreach (var icon in _icons.Values)
            icon.Dispose();
    }
}
