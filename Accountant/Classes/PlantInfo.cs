using System;
using System.Numerics;
using Accountant.Enums;
using Accountant.Gui;
using OtterLoc.Structs;

namespace Accountant.Classes;

public struct PlantInfo
{
    public const int PotsPerApartment = 2;
    public const int PotsPerChamber   = 2;

    public DateTime PlantTime;
    public DateTime LastTending;
    public Vector3  Position;
    public uint     PlantId;
    public bool     AccuratePlantTime;

    public bool Active()
        => PlantId != 0;

    public bool CloseEnough(Vector3 rhs)
        => (Position - rhs).LengthSquared() < 0.01;

    public DateTime FinishTime()
        => PlantId != 0 ? PlantTime.AddMinutes(Accountant.GameData.FindCrop(PlantId).Data.GrowTime) : DateTime.MinValue;

    public DateTime WiltingTime()
        => PlantId != 0 ? LastTending.AddMinutes(Accountant.GameData.FindCrop(PlantId).Data.WiltTime) : DateTime.MinValue;

    public DateTime DyingTime()
        => PlantId != 0 ? LastTending.AddMinutes(Accountant.GameData.FindCrop(PlantId).Data.WiltTime).AddDays(1) : DateTime.MinValue;

    public bool Update(uint itemId, DateTime? plantTime, DateTime? tendTime,
        DateTime? fertilizeTime, Vector3? position = null)
    {
        var ret = false;
        if (PlantId != itemId)
        {
            PlantId           = itemId;
            PlantTime         = DateTime.MinValue;
            LastTending       = DateTime.MinValue;
            AccuratePlantTime = false;
            ret               = true;
        }

        if (tendTime.HasValue && tendTime.Value != LastTending)
        {
            LastTending = tendTime.Value;
            if (PlantTime == DateTime.MinValue)
                PlantTime = LastTending;
            ret = true;
        }

        if (plantTime.HasValue && plantTime.Value != PlantTime)
        {
            AccuratePlantTime = true;
            PlantTime         = plantTime.Value;
            LastTending       = PlantTime;
            ret               = true;
        }

        if (position.HasValue)
        {
            Position = position.Value;
            return true;
        }

        return ret;
    }

    public (DateTime, DateTime, DateTime, ColorId, DateTime, bool) GetCropTimes(DateTime now)
    {
        if (PlantId == 0)
            return (DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, ColorId.NeutralText, DateTime.MinValue, false);

        var fin    = FinishTime();
        var wilt   = WiltingTime();
        var wither = wilt.AddDays(1);

        if (wither < now)
            return fin < wither 
                ? (DateTime.MinValue, DateTime.MaxValue, DateTime.MaxValue, ColorId.TextCropGrown, DateTime.MinValue, true) 
                : (DateTime.MaxValue, DateTime.MinValue, DateTime.MinValue, ColorId.TextCropWithered, DateTime.MinValue, true);

        if (fin < now)
            return (DateTime.MinValue, DateTime.MaxValue, DateTime.MaxValue, ColorId.TextCropGrown, DateTime.MinValue, true);

        if (fin < wither)
            return (fin, wilt < fin ? wilt : DateTime.MaxValue, DateTime.MaxValue, ColorId.TextCropGuaranteed, fin, true);

        if (wilt < now)
            return (fin, DateTime.MinValue, wither, ColorId.TextCropWilted, wither, AccuratePlantTime);

        return (fin, wilt, wither, ColorId.TextCropGrowing, wilt, AccuratePlantTime);
    }

    public static string GetPrivateName(ushort idx)
        => idx < PotsPerApartment
            ? $"{StringId.Apartment.Value()}, {StringId.CropPot.Value()} {idx + 1}"
            : $"{StringId.Chambers.Value()}, {StringId.CropPot.Value()} {idx + 1 - PotsPerApartment}";

    public static string GetPlotName(PlotSize size, ushort idx)
    {
        var s = size.OutdoorBeds();
        return idx < s
            ? $"{StringId.CropBed.Value()} {(idx >> 3) + 1}-{(idx & 0b111) + 1}"
            : $"{StringId.CropPot.Value()} {idx + 1 - s}";
    }
}
