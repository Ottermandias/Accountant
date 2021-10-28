using System;
using Accountant.Enums;

namespace Accountant.Data;

public class Plots
{
    internal PlotSize GetSize(InternalHousingZone zone, ushort plot)
    {
        if (plot > 60)
            throw new ArgumentOutOfRangeException($"Size of invalid housing plot {plot} requested.");

        var idx = plot - (plot > 30 ? 31 : 1);
        return zone switch
        {
            InternalHousingZone.Mist         => MistData[idx],
            InternalHousingZone.Goblet       => GobletData[idx],
            InternalHousingZone.LavenderBeds => LavenderBedsData[idx],
            InternalHousingZone.Shirogane    => ShiroganeData[idx],
            InternalHousingZone.Firmament    => FirmamentData[idx],
            _                                => throw new ArgumentException($"Size of invalid housing zone {zone} requested."),
        };
    }

    private static readonly PlotSize[] MistData =
    {
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.House,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.House,
    };

    private static readonly PlotSize[] LavenderBedsData =
    {
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.House,
    };

    private static readonly PlotSize[] GobletData =
    {
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Mansion,
    };

    private static readonly PlotSize[] ShiroganeData =
    {
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Mansion,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Mansion,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Cottage,
        PlotSize.House,
        PlotSize.Cottage,
        PlotSize.Mansion,
    };

    private static readonly PlotSize[] FirmamentData =
        { };
}
