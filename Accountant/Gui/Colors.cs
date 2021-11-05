using System;
using System.Collections.Generic;
using System.Linq;

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

    CollapsedBorder,
    NeutralText,
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
            ColorId.HeaderCropGrown      => "Some Crops Withered",
            ColorId.HeaderCropWithered   => "Some Crops Wilted",
            ColorId.HeaderCropWilted     => "Some Crops Grown Out",
            ColorId.HeaderCropGrowing    => "All Crops Growing",
            ColorId.HeaderCropGuaranteed => "Some Crops Self-Sufficient",
            ColorId.CollapsedBorder      => "Collapsed Header Border",
            ColorId.NeutralText          => "Default Text",
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
            ColorId.HeaderCropGrown    => "Used by the Crops header and the collapsed header if any crop is withered.",
            ColorId.HeaderCropWithered => "Used by the Crops header and the collapsed header if any crop is wilted and none is withered.",
            ColorId.HeaderCropWilted =>
                "Used by the Crops header and the collapsed header if any crop is grown out and none is wilted or withered.",
            ColorId.HeaderCropGrowing => "Used by the Crops header and the collapsed header if all crops are growing regularly.",
            ColorId.HeaderCropGuaranteed =>
                "Used by the Crops header and the collapsed header if all crops are growing regularly and require no tending anymore.",
            ColorId.CollapsedBorder => "Used by the collapsed header to give it a border to improve visibility.",
            ColorId.NeutralText     => "Used by all text that is not colored differently.",
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
        [ColorId.HeaderCropGrown]      = 0x8000A000,
        [ColorId.HeaderCropWithered]   = 0x800000A0,
        [ColorId.HeaderCropWilted]     = 0x80A000A0,
        [ColorId.HeaderCropGrowing]    = 0x8000A0A0,
        [ColorId.HeaderCropGuaranteed] = 0x80A0A000,
        [ColorId.CollapsedBorder]      = 0xFFFFFFFF,
        [ColorId.NeutralText]          = 0xFFFFFFFF,
        [ColorId.DisabledText]         = 0xFF808080,
        [ColorId.Background]           = 0xC0000000,
    }.Select(kvp => kvp.Value).ToArray();

    public static uint Default(this ColorId color)
        => Defaults[(byte)color];

    public static uint Value(this ColorId color)
        => Accountant.Config.Colors[color];

    public static ColorId CombineColor(this ColorId oldColor, ColorId newColor)
        => (ColorId)Math.Min((int)oldColor, (int)newColor);

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
            ColorId.CollapsedBorder      => ColorId.CollapsedBorder,
            ColorId.NeutralText          => ColorId.NeutralText,
            ColorId.Background           => ColorId.Background,
            _                            => throw new ArgumentOutOfRangeException(nameof(color), color, null),
        };
}
