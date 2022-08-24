using System;
using System.Runtime.InteropServices;
using Accountant.Internal;
using Dalamud.Memory;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Submersible.TimerSize)]
public unsafe struct SubmersibleTimer
{
    [FieldOffset(Offsets.Submersible.TimerTimeStamp)]
    public uint TimeStamp;

    [FieldOffset(Offsets.Submersible.TimerRawName)]
    public fixed byte RawName[Offsets.Submersible.TimerRawNameSize];

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
