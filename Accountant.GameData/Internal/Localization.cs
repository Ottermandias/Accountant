using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Accountant.Enums;
using Dalamud;
using Dalamud.Data;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using OtterLoc;
using OtterLoc.Enums;

namespace Accountant.Internal;

internal static class Localization
{
    private static bool _initialized;

    [Sheet("RetainerString")]
    private class StringSheet : ExcelRow
    {
        public SeString String { get; set; } = null!;

        public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
        {
            RowId    = parser.RowId;
            SubRowId = parser.SubRowId;
            String   = SeString.Parse(parser.ReadColumn<Lumina.Text.SeString>(1)!.RawData);
        }
    }

    [Sheet("GoldSaucerTalk")]
    private class GoldSaucerTalk : ExcelRow
    {
        public SeString String { get; set; } = null!;

        public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
        {
            RowId    = parser.RowId;
            SubRowId = parser.SubRowId;
            String   = SeString.Parse(parser.ReadColumn<Lumina.Text.SeString>(17)!.RawData);
        }
    }

    private static readonly Regex PlantingTextEn =
        new(@"Prepare the bed with (?<soil>.*?) and (a |an )?(?<seeds>.*?)\?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex PlantingTextFr = new(@"Planter (un |une )?(?<seeds>.*?) avec (?<soil>.*?).\?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex PlantingTextDe =
        new(@"(?<soil>.*?) verteilen und (einer |einem )?(?<seeds>.*?) aussäen\?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex PlantingTextJp = new(@"(?<soil>.*?)に(?<seeds>.*?)を植えます。よろしいですか？", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static StringParser PatchParser(ClientLanguage lang)
    {
        var (bed, patch) = lang switch
        {
            ClientLanguage.German   => (15, 5),
            ClientLanguage.French   => (23, 8),
            ClientLanguage.Japanese => (1, 4),
            _                       => (0, 9),
        };

        IList<string> Func(string s)
        {
            if (s.Length <= Math.Max(bed, patch))
                return Array.Empty<string>();

            return new[]
            {
                s.Substring(patch, 1),
                s.Substring(bed,   1),
            };
        }

        return new StringParser(Func);
    }

    private static void SetCropCommands(DataManager data)
    {
        var sheet = data.Excel.GetType().GetMethod("GetSheet", BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(StringSheet)).Invoke(data.Excel, new object?[]
            {
                "custom/001/cmndefhousinggardeningplant_00151",
                data.Language.ToLumina(),
                null,
            }) as ExcelSheet<StringSheet>;

        var addon = data.Excel.GetSheet<Addon>()!;

        LocalizationDict<StringId>.RegisterComparer(StringId.HarvestCrop,   sheet!.GetRow(6)!.String.TextValue);
        LocalizationDict<StringId>.RegisterComparer(StringId.TendCrop,      sheet.GetRow(4)!.String.TextValue);
        LocalizationDict<StringId>.RegisterComparer(StringId.FertilizeCrop, sheet.GetRow(3)!.String.TextValue);
        LocalizationDict<StringId>.RegisterComparer(StringId.RemoveCrop,    sheet.GetRow(5)!.String.TextValue);
        LocalizationDict<StringId>.RegisterComparer(StringId.DisposeCrop, (sheet.GetRow(11)!.String.Payloads[0] as TextPayload)!.Text!,
            MatchType.StartsWith);
        LocalizationDict<StringId>.RegisterComparer(StringId.PlantCrop, sheet.GetRow(2)!.String.TextValue);

        var matcher = SeStringMatcher.SinglePayloadComparer(data.Language, sheet.GetRow(7)!.String, ^1, ^1, ^1, 0);
        LocalizationDict<StringId>.Register(StringId.CropBeyondHope, matcher);
        matcher = SeStringMatcher.SinglePayloadComparer(data.Language, sheet.GetRow(8)!.String, ^1, ^1, ^1, 0);
        LocalizationDict<StringId>.Register(StringId.CropDoingWell, matcher);
        matcher = SeStringMatcher.SinglePayloadComparer(data.Language, sheet.GetRow(9)!.String, ^1, ^1, ^1, 0);
        LocalizationDict<StringId>.Register(StringId.CropBetterDays, matcher);
        matcher = SeStringMatcher.SinglePayloadComparer(data.Language, sheet.GetRow(10)!.String, ^1, ^3, ^1, 0);
        LocalizationDict<StringId>.Register(StringId.CropReady, matcher);
        matcher = SeStringMatcher.SinglePayloadComparer(data.Language, SeString.Parse(addon.GetRow(6413)!.Text.RawData), 0, 0, ^1, ^1);
        LocalizationDict<StringId>.Register(StringId.CropPrepareBed, matcher);

        LocalizationDict<StringId>.Register(StringId.PlantMatcher, SeStringParser.SpecificPayload(data.Language, 2, 3, 2, 3));
        LocalizationDict<StringId>.Register(StringId.PatchMatcher, PatchParser(data.Language));
        LocalizationDict<StringId>.Register(StringId.SeedMatcher,
            StringParser.FromRegex(data.Language, PlantingTextEn, PlantingTextFr, PlantingTextJp, PlantingTextDe, "seeds", "soil"));
    }

    private static readonly Regex WheelTextEn = new(@"Place (the )?(?<wheel>.*?) on the wheel stand\?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex WheelTextFr = new(@"Installer (la |le )?(?<wheel>.*?).\?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex WheelTextDe = new(@"(Das )? (?<wheel>.*?) wirklich in den Ätherrad-Ständer einsetzen\?",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex WheelTextJp = new(@"「(<?wheel>.*?)」を.*ホイールスタンドに設置します。.*よろしいですか？", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    private static readonly Regex JumboTextEn = new(@"number\s+(?<ticket>\d{4})", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    private static readonly Regex JumboTextFr = new(@"(?<ticket>\d{4})\s+pour", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private static readonly Regex JumboTextDe = new(@"Nummer\s+(?<ticket>\d{4})",
        RegexOptions.Compiled);

    private static readonly Regex JumboTextJp = new(@"(?<ticket>\d{4})番を", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    public static void Initialize(DataManager data)
    {
        if (_initialized)
            return;

        _initialized = true;
        var territories = data.Excel.GetSheet<TerritoryType>()!;
        var names       = data.Excel.GetSheet<PlaceName>()!;
        var addon       = data.Excel.GetSheet<Addon>()!;
        var goldSaucerTalk = data.Excel.GetType().GetMethod("GetSheet", BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(GoldSaucerTalk)).Invoke(data.Excel, new object?[]
            {
                "goldsaucertalk",
                data.Language.ToLumina(),
                null,
            }) as ExcelSheet<GoldSaucerTalk>;

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

        territory = territories.GetRow((uint)HousingZone.Empyreum)!;
        name      = names.GetRow(territory.PlaceName.Row)!;
        LocalizationDict<StringId>.Register(StringId.Empyreum, name.Name.RawString);

        LocalizationDict<StringId>.RegisterName(StringId.Unknown, data.Language, "Unknown", "不明", "Unbekannt", "Inconnu");

        LocalizationDict<StringId>.RegisterName(StringId.CropPatch, data.Language, "Patch",      "畑",     "Beet",       "Potager");
        LocalizationDict<StringId>.RegisterName(StringId.CropPot,   data.Language, "Flower Pot", "プランター", "Blumentopf", "Pot de Fleurs");
        LocalizationDict<StringId>.RegisterName(StringId.CropBed,   data.Language, "Bed",        "の畝",    "Furche",     "Emplacement");

        LocalizationDict<StringId>.RegisterName(StringId.Cottage,   data.Language, "Cottage",   "コテージ",  "Hütte",         "Maisonnette");
        LocalizationDict<StringId>.RegisterName(StringId.House,     data.Language, "House",     "ハウス",   "Haus",          "Pavillon");
        LocalizationDict<StringId>.RegisterName(StringId.Mansion,   data.Language, "Mansion",   "レジデンス", "Residenz",      "Villa");
        LocalizationDict<StringId>.RegisterName(StringId.Apartment, data.Language, "Apartment", "部屋",    "Wohnung",       "Appartement");
        LocalizationDict<StringId>.RegisterName(StringId.Chambers,  data.Language, "Chambers",  "ルーム",   "Zimmer",        "Chambre");
        LocalizationDict<StringId>.RegisterName(StringId.Completed, data.Language, "Completed", "完成",    "Abgeschlossen", "Complété");
        LocalizationDict<StringId>.RegisterName(StringId.Available, data.Language, "Available", "利用可能",  "Verfügbar",     "Disponible");
        LocalizationDict<StringId>.RegisterName(StringId.Machines,  data.Language, "Machines",  "マシン",   "Maschinen",     "Machines");
        LocalizationDict<StringId>.RegisterName(StringId.Retainers, data.Language, "Retainers", "リテイナー", "Gehilfen",      "Servants");

        LocalizationDict<StringId>.RegisterName(StringId.Airship, data.Language, "Airship", "飛行船", "Luftschiff", "Aéronef");
        LocalizationDict<StringId>.Register(StringId.Submersible,
            addon.GetRow(data.Language == ClientLanguage.Japanese ? 6881u : 6888u)!.Text.RawString);
        LocalizationDict<StringId>.Register(StringId.Retainer, addon.GetRow(6163)!.Text.RawString);

        LocalizationDict<StringId>.Register(StringId.WheelFilter,
            StringParser.FromRegex(data.Language, WheelTextEn, WheelTextFr, WheelTextJp, WheelTextDe, "wheel"));

        LocalizationDict<StringId>.Register(StringId.BuyMiniCactpotTicket,
            new StringMatcherLetters(goldSaucerTalk!.GetRow(16)!.String.TextValue));
        LocalizationDict<StringId>.Register(StringId.BuyJumboCactpotTicket,
            new StringMatcherLetters(SeString.Parse(addon!.GetRow(9276)!.Text.RawData).TextValue));
        LocalizationDict<StringId>.Register(StringId.FilterJumboCactpotTicket,
            StringParser.FromRegex(data.Language, JumboTextEn, JumboTextFr, JumboTextJp, JumboTextDe, "ticket"));
        SetCropCommands(data);
    }
}
