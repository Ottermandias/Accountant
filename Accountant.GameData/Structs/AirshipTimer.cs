using System;
using System.Runtime.InteropServices;
using Accountant.Internal;
using Dalamud.Memory;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Airship.TimerSize)]
public unsafe struct AirshipTimer
{
    [FieldOffset(Offsets.Airship.TimerTimeStamp)]
    public uint TimeStamp;

    [FieldOffset(Offsets.Airship.TimerRawName)]
    public fixed byte RawName[Offsets.Airship.TimerRawNameSize];

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
