using Lumina.Excel.Sheets;

namespace Accountant.Structs;

public readonly struct CropData(ushort growTime, ushort wiltTime, Item item, Item seed)
{
    public readonly ushort GrowTime = (ushort)(growTime * 60);
    public readonly ushort WiltTime = (ushort)(wiltTime * 60);
    public readonly Item   Item     = item;
    public readonly Item   Seed     = seed;

    public ushort WitherTime
        => (ushort)(WiltTime + 24 * 60);
}
