using System.Collections.Generic;
using Accountant.Structs;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace Accountant.Data;

public partial class Crops
{
    private readonly Dictionary<string, CropData>         _nameToData;
    private readonly Dictionary<uint, (CropData, string)> _idToData;

    internal (CropData Data, string Name) Find(uint itemId)
        => _idToData.TryGetValue(itemId, out var crop) ? crop : (_idToData[0u].Item1, string.Empty);

    internal CropData Find(string name)
        => _nameToData.TryGetValue(name.ToLowerInvariant(), out var crop) ? crop : _idToData[0u].Item1;

    internal Crops(IPluginLog log, IDataManager gameData)
    {
        if (_nameToData != null && _idToData != null)
            return;

        var sheet = gameData.GetExcelSheet<Item>()!;
        _nameToData = new Dictionary<string, CropData>(Data.Length * 4);
        _idToData   = new Dictionary<uint, (CropData, string)>(Data.Length * 2);
        foreach (var (growth, wilt, itemId, seedId) in Data)
        {
            if (!sheet.TryGetRow((uint)itemId, out var item))
            {
                log.Error($"Could not obtain item with id {itemId}");
                continue;
            }

            if (!sheet.TryGetRow((uint)seedId, out var seed))
            {
                log.Error($"Could not obtain item with id {seedId}");
                continue;
            }

            var crop = new CropData((ushort)growth, (ushort)wilt, item, seed);
            var name = item.Name.ToString();
            _idToData[(uint)itemId] = (crop, name);
            _idToData[(uint)seedId] = (crop, name);

            _nameToData[name.ToLowerInvariant()]                     = crop;
            _nameToData[seed.Name.ToString().ToLowerInvariant()]     = crop;
            _nameToData[item.Singular.ToString().ToLowerInvariant()] = crop;
            _nameToData[seed.Singular.ToString().ToLowerInvariant()] = crop;
        }
    }
}