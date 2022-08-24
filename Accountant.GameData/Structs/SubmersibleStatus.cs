using System;
using System.Runtime.InteropServices;
using Accountant.Internal;
using Dalamud.Memory;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Submersible.StatusSize)]
public unsafe struct SubmersibleStatus
{
    [FieldOffset(Offsets.Submersible.StatusTimeStamp)]
    public uint TimeStamp;

    [FieldOffset(Offsets.Submersible.StatusRawName)]
    public fixed byte RawName[Offsets.Submersible.StatusRawNameSize];

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
