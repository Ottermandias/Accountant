using System;
using Accountant.Enums;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant.SeFunctions;

public sealed class PositionInfoAddress : SeAddressBase
{
    public ushort Ward
        => Info.Ward;

    public InternalHousingZone Zone
        => Info.Zone;

    public ushort House
        => Info.House;

    public bool Subdivision
        => Info.Subdivision;

    public byte Plot
        => Info.Plot;

    public HousingFloor HousingFloor
        => Info.HousingFloor;

    public PositionInfoAddress(IPluginLog log, ISigScanner sigScanner)
        : base(log, sigScanner, Signatures.PositionInfo)
    { }

    private readonly unsafe struct PositionInfo
    {
        private readonly byte* _address;

        private PositionInfo(byte* address)
            => _address = address;

        public static implicit operator PositionInfo(IntPtr ptr)
            => new((byte*)ptr);

        public static implicit operator PositionInfo(byte* ptr)
            => new(ptr);

        public static implicit operator bool(PositionInfo ptr)
            => ptr._address != null;

        public ushort House
            => (ushort)(_address == null || !InHouse ? 0 : *(ushort*)(_address + 0x96A0) + 1);

        public ushort Ward
            => (ushort)(_address == null ? 0 : *(ushort*)(_address + 0x96A2) + 1);

        public bool Subdivision
            => _address != null && *(_address + 0x96A9) == 2;

        public InternalHousingZone Zone
            => _address == null ? InternalHousingZone.Unknown : *(InternalHousingZone*)(_address + 0x96A4);

        public byte Plot
            => (byte)(_address == null || InHouse ? 0 : *(_address + 0x96A8) + 1);

        public HousingFloor HousingFloor
            => _address == null ? HousingFloor.Unknown : *(HousingFloor*)(_address + 0x9704);

        private bool InHouse
            => *(_address + 0x96A9) == 0;
    }

    private unsafe PositionInfo Info
    {
        get
        {
            var intermediate = *(byte***)Address;
            return intermediate == null ? null : *intermediate;
        }
    }
}
