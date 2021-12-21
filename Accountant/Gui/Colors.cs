using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Classes;
using Accountant.Timers;

namespace Accountant.Gui;

public enum ColorId : byte
{
    TextObjectsAway,
    TextObjectsMixed,
    TextObjectsHome,

    HeaderObjectsAway,
    HeaderObjectsMixed,
    HeaderObjectsHome,

    TextCropWithered,
    TextCropWilted,
    TextCropGrown,
    TextCropGrowing,
    TextCropGuaranteed,

    HeaderCropWithered,
    HeaderCropWilted,
    HeaderCropGrown,
    HeaderCropGrowing,
    HeaderCropGuaranteed,

    TextLeveCap,
    TextLeveWarning,
    HeaderLeveCap,
    HeaderLeveWarning,

    CollapsedBorder,
    NeutralText,
    NeutralHeader,
    DisabledText,
    Background,
}

public static class ColorIdExtensions
{
    public static string Name(this ColorId id)
        => id switch
        {
            ColorId.TextObjectsAway      => "All Objects Underway",
            ColorId.TextObjectsMixed     => "Some Objects Underway",
            ColorId.TextObjectsHome      => "No Objects Underway",
            ColorId.HeaderObjectsAway    => "All Objects Underway (Header)",
            ColorId.HeaderObjectsMixed   => "Some Objects Underway (Header)",
            ColorId.HeaderObjectsHome    => "No Objects Underway (Header)",
            ColorId.TextCropWithered     => "Crop Withered",
            ColorId.TextCropWilted       => "Crop Wilted",
            ColorId.TextCropGrown        => "Crop Grown Out",
            ColorId.TextCropGrowing      => "Crop Growing",
            ColorId.TextCropGuaranteed   => "Crop Self-Sufficient",
            ColorId.HeaderCropWithered   => "Some Crops Withered (Header)",
            ColorId.HeaderCropWilted     => "Some Crops Wilted (Header)",
            ColorId.HeaderCropGrown      => "Some Crops Grown Out (Header)",
            ColorId.HeaderCropGrowing    => "All Crops Growing (Header)",
            ColorId.HeaderCropGuaranteed => "Some Crops Self-Sufficient (Header)",
            ColorId.TextLeveCap          => "Leve Allowances Capped",
            ColorId.TextLeveWarning      => "Leve Allowances Nearing Cap",
            ColorId.HeaderLeveCap        => "Leve Allowances Capped (Header)",
            ColorId.HeaderLeveWarning    => "Leve Allowances Nearing Cap (Header)",
            ColorId.CollapsedBorder      => "Collapsed Header Border",
            ColorId.NeutralText          => "Default Text",
            ColorId.NeutralHeader        => "Default Header",
            ColorId.DisabledText         => "Disabled Text",
            ColorId.Background           => "Background",
            _                            => throw new ArgumentOutOfRangeException(nameof(id), id, null),
        };

    public static string Description(this ColorId id)
        => id switch
        {
            ColorId.TextObjectsAway => "Used by player or free company names if all their retainers, airships or submersibles are away.",
            ColorId.TextObjectsMixed =>
                "Used by player or free company names if some their retainers, airships or submersibles are away and some are available.",
            ColorId.TextObjectsHome   => "Used by player or free company names if all their retainers, airships or submersibles are available.",
            ColorId.HeaderObjectsAway => "Used by the Retainers and Machines headers if all retainers, airships or submersibles are away.",
            ColorId.HeaderObjectsMixed =>
                "Used by the Retainers and Machines headers if some retainers, airships or submersibles are away and some are available.",
            ColorId.HeaderObjectsHome => "Used by the Retainers and Machines headers if all retainers, airships or submersibles are available.",
            ColorId.TextCropWithered =>
                "Used by single crops if they are withered.\nUsed by plot or player names if any of their crops is withered.",
            ColorId.TextCropWilted =>
                "Used by single crops if they are wilted.\nUsed by plot or player names if any of their crops are wilted and none are withered.",
            ColorId.TextCropGrown =>
                "Used by single crops if they are grown out.\nUsed by plot or player names if any of their crops are grown out and none are wilted or withered",
            ColorId.TextCropGrowing =>
                "Used by single crops if they are growing regularly.\nUsed by plot or player names if all of their crops are growing regularly.",
            ColorId.TextCropGuaranteed =>
                "Used by single crops if they are growing regularly and require no tending anymore.\nUsed by plot or player names if all of their crops are growing regularly and require no tending anymore.",
            ColorId.HeaderCropWithered => "Used by the Crops header and the collapsed header if any crop is withered.",
            ColorId.HeaderCropWilted   => "Used by the Crops header and the collapsed header if any crop is wilted and none is withered.",
            ColorId.HeaderCropGrown =>
                "Used by the Crops header and the collapsed header if any crop is grown out and none is wilted or withered.",
            ColorId.HeaderCropGrowing => "Used by the Crops header and the collapsed header if all crops are growing regularly.",
            ColorId.HeaderCropGuaranteed =>
                "Used by the Crops header and the collapsed header if all crops are growing regularly and require no tending anymore.",
            ColorId.TextLeveCap =>
                $"Used by Leve Allowances per player when you have accrued more than {Leve.AllowanceError} allowances.",
            ColorId.TextLeveWarning =>
                $"Used by Leve Allowances per player when you have accrued more than than {Accountant.Config.LeveWarning} allowances. (Configure in Settings)",
            ColorId.HeaderLeveCap =>
                $"Used by Leve Allowances header when any player has accrued more than {Leve.AllowanceError} allowances.",
            ColorId.HeaderLeveWarning =>
                $"Used by Leve Allowances header when any player has accrued more than {Accountant.Config.LeveWarning} allowances. (Configure in Settings)",
            ColorId.CollapsedBorder => "Used by the collapsed header to give it a border to improve visibility.",
            ColorId.NeutralText     => "Used by all text that is not colored differently.",
            ColorId.NeutralHeader   => "Used by all headers that are not colored differently.",
            ColorId.DisabledText    => "Used by disabled retainers or machines.",
            ColorId.Background      => "Used for the background color.",
            _                       => throw new ArgumentOutOfRangeException(nameof(id), id, null),
        };

    public static readonly IReadOnlyList<uint> Defaults = new SortedList<ColorId, uint>
    {
        [ColorId.TextObjectsAway]      = 0xFF2020D0,
        [ColorId.TextObjectsMixed]     = 0xFF20D0D0,
        [ColorId.TextObjectsHome]      = 0xFF20D020,
        [ColorId.HeaderObjectsAway]    = 0x800000A0,
        [ColorId.HeaderObjectsMixed]   = 0x8000A0A0,
        [ColorId.HeaderObjectsHome]    = 0x8000A000,
        [ColorId.TextCropGrown]        = 0xFF20D020,
        [ColorId.TextCropWithered]     = 0xFF2020D0,
        [ColorId.TextCropWilted]       = 0xFFD020D0,
        [ColorId.TextCropGrowing]      = 0xFF20D0D0,
        [ColorId.TextCropGuaranteed]   = 0xFFD0D020,
        [ColorId.HeaderCropWithered]   = 0x800000A0,
        [ColorId.HeaderCropWilted]     = 0x80A000A0,
        [ColorId.HeaderCropGrown]      = 0x8000A000,
        [ColorId.HeaderCropGrowing]    = 0x8000A0A0,
        [ColorId.HeaderCropGuaranteed] = 0x80A0A000,
        [ColorId.TextLeveCap]          = 0xFF2020D0,
        [ColorId.TextLeveWarning]      = 0xFF20D0D0,
        [ColorId.HeaderLeveCap]        = 0x800000A0,
        [ColorId.HeaderLeveWarning]    = 0x8000A0A0,
        [ColorId.CollapsedBorder]      = 0xFFFFFFFF,
        [ColorId.NeutralText]          = 0xFFFFFFFF,
        [ColorId.NeutralHeader]        = 0x4F969696,
        [ColorId.DisabledText]         = 0xFF808080,
        [ColorId.Background]           = 0xC0000000,
    }.Select(kvp => kvp.Value).ToArray();

    public static uint Default(this ColorId color)
        => Defaults[(byte)color];

    public static uint Value(this ColorId color)
        => Accountant.Config.Colors[color];

    public static ColorId Combine(this ColorId oldColor, ColorId newColor)
        => (ColorId)Math.Min((int)oldColor, (int)newColor);

    public static ColorId CombineInverse(this ColorId oldColor, ColorId newColor)
        => (ColorId)Math.Max((int)oldColor, (int)newColor);

    public static ColorId TextToHeader(this ColorId color)
        => color switch
        {
            ColorId.TextObjectsAway      => ColorId.HeaderObjectsAway,
            ColorId.TextObjectsMixed     => ColorId.HeaderObjectsMixed,
            ColorId.TextObjectsHome      => ColorId.HeaderObjectsHome,
            ColorId.HeaderObjectsAway    => ColorId.HeaderObjectsAway,
            ColorId.HeaderObjectsMixed   => ColorId.HeaderObjectsMixed,
            ColorId.HeaderObjectsHome    => ColorId.HeaderObjectsHome,
            ColorId.TextCropWithered     => ColorId.HeaderCropWithered,
            ColorId.TextCropWilted       => ColorId.HeaderCropWilted,
            ColorId.TextCropGrown        => ColorId.HeaderCropGrown,
            ColorId.TextCropGrowing      => ColorId.HeaderCropGrowing,
            ColorId.TextCropGuaranteed   => ColorId.HeaderCropGuaranteed,
            ColorId.HeaderCropWithered   => ColorId.HeaderCropWithered,
            ColorId.HeaderCropWilted     => ColorId.HeaderCropWilted,
            ColorId.HeaderCropGrown      => ColorId.HeaderCropGrown,
            ColorId.HeaderCropGrowing    => ColorId.HeaderCropGrowing,
            ColorId.HeaderCropGuaranteed => ColorId.HeaderCropGuaranteed,
            ColorId.TextLeveCap          => ColorId.HeaderLeveCap,
            ColorId.TextLeveWarning      => ColorId.HeaderLeveWarning,
            ColorId.HeaderLeveCap        => ColorId.HeaderLeveCap,
            ColorId.HeaderLeveWarning    => ColorId.HeaderLeveWarning,
            ColorId.CollapsedBorder      => ColorId.CollapsedBorder,
            ColorId.NeutralText          => ColorId.NeutralHeader,
            ColorId.NeutralHeader        => ColorId.NeutralHeader,
            ColorId.Background           => ColorId.Background,
            ColorId.DisabledText         => ColorId.NeutralHeader,
            _                            => throw new ArgumentOutOfRangeException(nameof(color), color, null),
        };
}
