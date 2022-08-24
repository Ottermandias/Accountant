using System;
using System.Runtime.InteropServices;
using Accountant.Internal;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Explicit, Size = Offsets.Retainer.Size)]
public unsafe struct SeRetainer
{
    [FieldOffset(Offsets.Retainer.RetainerId)]
    public ulong RetainerID;

    [FieldOffset(Offsets.Retainer.Name)]
    private fixed byte _name[Offsets.Retainer.NameSize];

    [FieldOffset(Offsets.Retainer.ClassJob)]
    public byte ClassJob;

    [FieldOffset(Offsets.Retainer.Gil)]
    public uint Gil;

    [FieldOffset(Offsets.Retainer.VentureId)]
    public uint VentureID;

    [FieldOffset(Offsets.Retainer.VentureTimestamp)]
    public uint VentureCompleteTimeStamp;

    public bool Available
        => ClassJob != 0;

    public DateTime VentureComplete
        => Helpers.DateFromTimeStamp(VentureCompleteTimeStamp);

    public SeString Name
    {
        get
        {
            fixed (byte* name = _name)
            {
                return MemoryHelper.ReadSeStringNullTerminated((IntPtr)name);
            }
        }
    }
}
