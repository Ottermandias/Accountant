using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Data;

public class Wheels
{
    private const int NumWheels = 45;

    private readonly Dictionary<uint, (Item Item, byte Grade)>   _idToItem   = new(NumWheels);
    private readonly Dictionary<string, (Item Item, byte Grade)> _nameToItem = new(NumWheels);

    internal (Item, byte Grade) Find(uint itemId)
        => _idToItem.TryGetValue(itemId, out var wheel) ? wheel : (new Item(), (byte)0);

    internal (Item, byte Grade) Find(string name)
        => _nameToItem.TryGetValue(name.ToLowerInvariant(), out var wheel) ? wheel : (new Item(), (byte)0);

    private static readonly Regex WheelRegex = new(@"^grade (?<grade>\d) wheel of", RegexOptions.Compiled);

    internal Wheels(DataManager gameData)
    {
        var items     = gameData.GetExcelSheet<Item>(ClientLanguage.English)!;
        var itemsLang = gameData.GetExcelSheet<Item>()!;
        foreach (var item in items)
        {
            var name  = item.Name.RawString.ToLowerInvariant();
            var match = WheelRegex.Match(name);
            if (!match.Success)
                continue;

            var grade    = (byte)(match.Groups["grade"].Value[0] - '0');
            var itemLang = itemsLang.GetRow(item.RowId)!;
            name = itemLang.Name.RawString.ToLowerInvariant();
            var singular = itemLang.Singular.RawString.ToLowerInvariant();
            _idToItem.TryAdd(itemLang.RowId, (itemLang, grade));
            _nameToItem.TryAdd(name,     (itemLang, grade));
            _nameToItem.TryAdd(singular, (itemLang, grade));
        }
    }
}