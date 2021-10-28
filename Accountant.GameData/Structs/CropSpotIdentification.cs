using System.Numerics;
using Accountant.Enums;

namespace Accountant.Structs;

public struct CropSpotIdentification
{
    public string              PlayerName;
    public Vector3             Position;
    public CropSpotType        Type;
    public InternalHousingZone Zone;
    public byte                ServerId;
    public byte                Ward;
    public byte                Plot;
    public byte                Patch;
    public byte                Bed;

    public static readonly CropSpotIdentification Invalid = new() { Type = CropSpotType.Invalid };
}
