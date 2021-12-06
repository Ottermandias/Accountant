using System.Collections.Generic;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Data;

public class Maps
{
    private readonly Dictionary<uint, Item> _idToItem;

    internal Item? Find(uint itemId)
        => _idToItem.TryGetValue(itemId, out var map) ? map : null;

    internal Maps(DataManager gameData)
    {
        var items = gameData.GetExcelSheet<Item>()!;
        _idToItem = new Dictionary<uint, Item>()
        {
            [12241] = items.GetRow(12241)!,
            [6691]  = items.GetRow(6691)!,
            [12243] = items.GetRow(12243)!,
            [17835] = items.GetRow(17835)!,
            [17836] = items.GetRow(17836)!,
            [26744] = items.GetRow(26744)!,
            [6689]  = items.GetRow(6689)!,
            [6688]  = items.GetRow(6688)!,
            [6692]  = items.GetRow(6692)!,
            [19770] = items.GetRow(19770)!,
            [6690]  = items.GetRow(6690)!,
            [12242] = items.GetRow(12242)!,
            [26745] = items.GetRow(26745)!,
            [33328] = items.GetRow(33328)!,
            [24794] = items.GetRow(24794)!,
            [7884]  = items.GetRow(7884)!,
            [8156]  = items.GetRow(8156)!,
            [9900]  = items.GetRow(9900)!,
            [36611] = items.GetRow(36611)!,
            [36612] = items.GetRow(36612)!,
        };
    }
}
