using Accountant.Enums;
using Dalamud;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;
using OtterLoc;

namespace Accountant.Internal;

internal static class Localization
{
    private static bool _initialized = false;

    public static void Initialize(DataManager data)
    {
        if (_initialized)
            return;

        _initialized = true;
        var territories = data.Excel.GetSheet<TerritoryType>()!;
        var names       = data.Excel.GetSheet<PlaceName>()!;
        var addon       = data.Excel.GetSheet<Addon>()!;

        var territory = territories.GetRow((uint)HousingZone.Mist)!;
        var name      = names.GetRow(territory.PlaceName.Row)!;
        LocalizationDict<StringId>.Register(StringId.Mist, name.Name.RawString);

        territory = territories.GetRow((uint)HousingZone.LavenderBeds)!;
        name      = names.GetRow(territory.PlaceName.Row)!;
        LocalizationDict<StringId>.Register(StringId.LavenderBeds, name.Name.RawString);

        territory = territories.GetRow((uint)HousingZone.Goblet)!;
        name      = names.GetRow(territory.PlaceName.Row)!;
        LocalizationDict<StringId>.Register(StringId.Goblet, name.Name.RawString);

        territory = territories.GetRow((uint)HousingZone.Shirogane)!;
        name      = names.GetRow(territory.PlaceName.Row)!;
        LocalizationDict<StringId>.Register(StringId.Shirogane, name.Name.RawString);

        //territory = territories.GetRow((uint)HousingZone.Firmament)!;
        //name      = names.GetRow(territory.PlaceName.Row)!;
        //LocalizationDict<StringId>.Register(StringId.Firmament, name.Name.RawString);

        LocalizationDict<StringId>.RegisterName(StringId.Firmament, data.Language, "Firmament", "蒼天街", "Himmelsstadt", "Azurée");
        LocalizationDict<StringId>.RegisterName(StringId.Unknown,   data.Language, "Unknown",   "不明",  "Unbekannt",    "Inconnu");

        LocalizationDict<StringId>.RegisterName(StringId.CropPatch, data.Language, "Patch", "畑",  "Beet",   "Potager");
        LocalizationDict<StringId>.RegisterName(StringId.CropBed,   data.Language, "Bed",   "の畝", "Furche", "Emplacement");

        LocalizationDict<StringId>.RegisterName(StringId.Cottage,   data.Language, "Cottage",   "コテージ",  "Hütte",    "Maisonnette");
        LocalizationDict<StringId>.RegisterName(StringId.House,     data.Language, "House",     "ハウス",   "Haus",     "Pavillon");
        LocalizationDict<StringId>.RegisterName(StringId.Mansion,   data.Language, "Mansion",   "レジデンス", "Residenz", "Villa");
        LocalizationDict<StringId>.RegisterName(StringId.Apartment, data.Language, "Apartment", "部屋",    "Wohnung",  "Appartement");
        LocalizationDict<StringId>.RegisterName(StringId.Chambers,  data.Language, "Chambers",  "ルーム",   "Zimmer",   "Chambre");


        LocalizationDict<StringId>.RegisterName(StringId.Airship, data.Language, "Airship", "飛行船", "Luftschiff", "Aéronef");
        LocalizationDict<StringId>.Register(StringId.Submersible,
            addon.GetRow(data.Language == ClientLanguage.Japanese ? 6881u : 6888u)!.Text.RawString);
        LocalizationDict<StringId>.Register(StringId.Retainer, addon.GetRow(6163)!.Text.RawString);
    }
}
