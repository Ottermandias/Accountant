using System;
using System.Linq;
using Accountant.Enums;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Accountant.Data;

public class Plots
{
    internal PlotSize GetSize(InternalHousingZone zone, ushort plot)
    {
        var data = zone switch
        {
            InternalHousingZone.Mist         => _mistData,
            InternalHousingZone.Goblet       => _gobletData,
            InternalHousingZone.LavenderBeds => _lavenderBedsData,
            InternalHousingZone.Shirogane    => _shiroganeData,
            InternalHousingZone.Empyreum    => _empyreumData,
            _                                => throw new ArgumentException($"Size of invalid housing zone {zone} requested."),
        };
        --plot;
        if (plot >= data.Length)
            throw new ArgumentOutOfRangeException($"Size of invalid housing plot {plot} requested.");

        return data[plot];
    }

    internal int GetNumWards(InternalHousingZone _)
        => 30;

    internal int GetNumPlots(InternalHousingZone zone)
    {
        var data = zone switch
        {
            InternalHousingZone.Mist         => _mistData,
            InternalHousingZone.Goblet       => _gobletData,
            InternalHousingZone.LavenderBeds => _lavenderBedsData,
            InternalHousingZone.Shirogane    => _shiroganeData,
            InternalHousingZone.Empyreum    => _empyreumData,
            _                                => throw new ArgumentException($"Size of invalid housing zone {zone} requested."),
        };
        return data.Length;
    }

    internal Plots(DataManager data)
    {
        var sheet = data.GetExcelSheet<HousingLandSet>()!;
        _mistData         = sheet.GetRow(0)!.PlotSize.Select(b => (PlotSize)b).ToArray();
        _lavenderBedsData = sheet.GetRow(1)!.PlotSize.Select(b => (PlotSize)b).ToArray();
        _gobletData       = sheet.GetRow(2)!.PlotSize.Select(b => (PlotSize)b).ToArray();
        _shiroganeData    = sheet.GetRow(3)!.PlotSize.Select(b => (PlotSize)b).ToArray();
        _empyreumData     = sheet.GetRow(4)!.PlotSize.Select(b => (PlotSize)b).ToArray();
    }

    private readonly PlotSize[] _mistData;
    private readonly PlotSize[] _lavenderBedsData;
    private readonly PlotSize[] _gobletData;
    private readonly PlotSize[] _shiroganeData;
    private readonly PlotSize[] _empyreumData;
}
