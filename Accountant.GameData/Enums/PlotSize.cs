namespace Accountant.Enums;

// Cottage = Small
// House   = Medium
// Mansion = Large
public enum PlotSize : byte
{
    Cottage,
    House,
    Mansion,
}


public static class PlotSizeExtensions
{
    public static int TotalBeds(this PlotSize s)
        => s switch
        {
            PlotSize.Cottage => 8 + 2,
            PlotSize.House   => 2 * 8 + 3,
            PlotSize.Mansion => 3 * 8 + 4,
            _                => 0,
        };

    public static int OutdoorBeds(this PlotSize s)
        => s switch
        {
            PlotSize.Cottage => 8,
            PlotSize.House   => 2 * 8,
            PlotSize.Mansion => 3 * 8,
            _                => 0,
        };

    public static int IndoorBeds(this PlotSize s)
        => s switch
        {
            PlotSize.Cottage => 2,
            PlotSize.House   => 3,
            PlotSize.Mansion => 4,
            _                => 0,
        };
}