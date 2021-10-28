using Lumina.Excel.GeneratedSheets;

namespace Accountant.Structs;

public readonly struct CropData
{
    public readonly ushort GrowTime;
    public readonly ushort WiltTime;
    public readonly Item   Item;
    public readonly Item   Seed;

    public ushort WitherTime
        => (ushort)(WiltTime + 24 * 60);

    public CropData(ushort growTime, ushort wiltTime, Item item, Item seed)
    {
        GrowTime = (ushort)(growTime * 60);
        WiltTime = (ushort)(wiltTime * 60);
        Item     = item;
        Seed     = seed;
    }
}
