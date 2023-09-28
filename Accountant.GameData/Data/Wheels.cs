using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Data;

public class Wheels
{
    private const int NumWheels = 45;

    private readonly Dictionary<uint, (Item Item, string Name, byte Grade)>   _idToItem   = new(NumWheels);
    private readonly Dictionary<string, (Item Item, string Name, byte Grade)> _nameToItem = new(NumWheels);

    internal (Item, string Name, byte Grade) Find(uint itemId)
        => _idToItem.TryGetValue(itemId, out var wheel) ? wheel : (new Item(), string.Empty, (byte)0);

    internal (Item, string Name, byte Grade) Find(string name)
        => _nameToItem.TryGetValue(name.ToLowerInvariant(), out var wheel) ? wheel : (new Item(), string.Empty, (byte)0);

    private static readonly Regex WheelRegex = new(@"^grade (?<grade>\d) wheel of", RegexOptions.Compiled);
    private static readonly Regex PrimedWheelRegex = new(@"^primed grade (?<grade>\d) wheel of", RegexOptions.Compiled);

    internal Wheels(IDataManager gameData)
    {
        var items        = gameData.GetExcelSheet<Item>(ClientLanguage.English)!;
        var itemsLang    = gameData.GetExcelSheet<Item>()!;
        var primedWheels = new List<(string, uint)>(50);
        var englishDict  = new Dictionary<string, (Item Item, string Name, byte Grade)>(50);
        foreach (var item in items)
        {
            var englishName  = item.Name.RawString.ToLowerInvariant();
            var match = WheelRegex.Match(englishName);
            if (!match.Success)
            {
                match = PrimedWheelRegex.Match(englishName);
                if (match.Success)
                    primedWheels.Add((englishName.Replace("primed ", ""), item.RowId));
                continue;
            }

            var grade    = (byte)(match.Groups["grade"].Value[0] - '0');
            var itemLang = itemsLang.GetRow(item.RowId)!;
            var name = SeString.Parse(itemLang.Name.RawData).TextValue;
            var singular = SeString.Parse(itemLang.Singular.RawData).TextValue.ToLowerInvariant();
            _idToItem.TryAdd(itemLang.RowId, (itemLang, name, grade));
            _nameToItem.TryAdd(name.ToLowerInvariant(), (itemLang, name, grade));
            _nameToItem.TryAdd(singular,                (itemLang, name, grade));
            englishDict.TryAdd(englishName,             (itemLang, name, grade));
        }

        foreach (var (englishName, primedWheel) in primedWheels)
        {
            var itemLang     = itemsLang.GetRow(primedWheel)!;
            var fullName     = SeString.Parse(itemLang.Name.RawData).TextValue.ToLowerInvariant();
            var singular     = SeString.Parse(itemLang.Singular.RawData).TextValue.ToLowerInvariant();
            if (!englishDict.TryGetValue(englishName, out var data))
                continue;
            _nameToItem.TryAdd(fullName, data);
            _nameToItem.TryAdd(singular, data);
        }
    }
}
