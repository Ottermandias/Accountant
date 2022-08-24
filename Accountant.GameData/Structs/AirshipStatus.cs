using System;
using System.Runtime.InteropServices;
using System.Threading;
using Accountant.Internal;
using Dalamud.Memory;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Airship.StatusSize)]
public unsafe struct AirshipStatus
{
    [FieldOffset(Offsets.Airship.StatusTimeStamp)]
    public uint TimeStamp;

    [FieldOffset(Offsets.Airship.StatusRawName)]
    public fixed byte RawName[Offsets.Airship.StatusRawNameSize];

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
