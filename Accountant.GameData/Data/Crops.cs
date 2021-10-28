using System;
using System.Collections.Generic;
using Accountant.Structs;
using Dalamud.Data;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Data;

public partial class Crops
{
    private readonly Dictionary<string, CropData>         _nameToData;
    private readonly Dictionary<uint, (CropData, string)> _idToData;

    internal (CropData Data, string Name) Find(uint itemId)
        => _idToData.TryGetValue(itemId, out var crop) ? crop : (_idToData![0u].Item1, string.Empty);

    internal CropData Find(string name)
        => _nameToData.TryGetValue(name.ToLowerInvariant(), out var crop) ? crop : _idToData![0u].Item1;

    internal Crops(DataManager gameData)
    {
        if (_nameToData != null && _idToData != null)
            return;

        var sheet = gameData.GetExcelSheet<Item>()!;
        _nameToData = new Dictionary<string, CropData>(Data.Length * 4);
        _idToData   = new Dictionary<uint, (CropData, string)>(Data.Length * 2);
        foreach (var (growth, wilt, itemId, seedId) in Data)
        {
            var item = sheet.GetRow((uint)itemId);
            var seed = sheet.GetRow((uint)seedId);
            if (item == null)
            {
                PluginLog.Error($"Could not obtain item with id {itemId}");
                continue;
            }

            if (seed == null)
            {
                PluginLog.Error($"Could not obtain item with id {seedId}");
                continue;
            }

            var crop = new CropData((ushort)growth, (ushort)wilt, item, seed);
            var name = item.Name.ToString();
            _idToData[(uint)itemId] = (crop, name);
            _idToData[(uint)seedId] = (crop, name);

            _nameToData[name]                     = crop;
            _nameToData[seed.Name.ToString()]     = crop;
            _nameToData[item.Singular.ToString()] = crop;
            _nameToData[seed.Singular.ToString()] = crop;
        }
    }
}
