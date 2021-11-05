using System;
using OtterLoc.Structs;

namespace Accountant.Enums;

// The housing zone as given by the current position struc.t
public enum InternalHousingZone : byte
{
    Unknown      = 0,
    Mist         = 83,
    Goblet       = 85,
    LavenderBeds = 84,
    Shirogane    = 129,
    Firmament    = 254, // currently unknown
}

public static class InternalHousingZoneExtensions
{
    public static string ToName(this InternalHousingZone z)
    {
        return z switch
        {
            InternalHousingZone.Unknown      => StringId.Unknown.Value(),
            InternalHousingZone.Mist         => StringId.Mist.Value(),
            InternalHousingZone.Goblet       => StringId.Goblet.Value(),
            InternalHousingZone.LavenderBeds => StringId.LavenderBeds.Value(),
            InternalHousingZone.Shirogane    => StringId.Shirogane.Value(),
            InternalHousingZone.Firmament    => StringId.Firmament.Value(),
            _                                => throw new ArgumentOutOfRangeException(nameof(z), z, null),
        };
    }
}
