using System;
using Accountant.Internal;
using Dalamud.Game;

namespace Accountant.SeFunctions;

public sealed class StaticSquadronContainer : SeAddressBase
{
    public StaticSquadronContainer(SigScanner sigScanner)
        : base(sigScanner, "8B 3D ?? ?? ?? ?? 8B D8 3B F8")
    { }

    public unsafe DateTime MissionEnd
        => Address == IntPtr.Zero ? DateTime.MaxValue : Helpers.DateFromTimeStamp(*(uint*)Address);

    public unsafe DateTime TrainingEnd
        => Address == IntPtr.Zero ? DateTime.MaxValue : Helpers.DateFromTimeStamp(*(uint*) (Address + 4));

    public unsafe ushort MissionId
        => Address == IntPtr.Zero ? ushort.MaxValue : *(ushort*) (Address + 8);

    public unsafe ushort TrainingId
        => Address == IntPtr.Zero ? ushort.MaxValue : *(ushort*)(Address + 10);

    public unsafe bool NewRecruits
        => Address != IntPtr.Zero && *(byte*)(Address + 12) != 0;
}