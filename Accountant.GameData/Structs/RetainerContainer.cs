using System.Runtime.InteropServices;

namespace Accountant.Structs;

[StructLayout(LayoutKind.Sequential, Size = Offsets.Retainer.Size * Offsets.Retainer.ContainerCount + Offsets.Retainer.ContainerData)]
public unsafe struct RetainerContainer
{
    public fixed byte Retainers[Offsets.Retainer.Size * Offsets.Retainer.ContainerCount];
    public fixed byte DisplayOrder[10];
    public       byte Ready;
    public       byte RetainerCount;
}
