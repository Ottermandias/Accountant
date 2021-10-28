using System;
using System.Runtime.InteropServices;
using Accountant.Internal;
using Dalamud.Memory;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x24)]
public unsafe struct AirshipStatus
{
    [FieldOffset(0x08)]
    public uint TimeStamp;

    [FieldOffset(0x10)]
    public fixed byte RawName[0x10];

    public string Name
    {
        get
        {
            fixed (byte* name = RawName)
            {
                return MemoryHelper.ReadStringNullTerminated((IntPtr)name);
            }
        }
    }

    public DateTime Date
        => Helpers.DateFromTimeStamp(TimeStamp);
}
