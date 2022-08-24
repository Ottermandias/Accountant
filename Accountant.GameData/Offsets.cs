using System.Data;
using System.Drawing;
using System.Reflection.Metadata;

namespace Accountant;

public static class Offsets
{
    public static class Submersible
    {
        public const int TimerSize        = 0x24;
        public const int TimerTimeStamp   = 0x00;
        public const int TimerRawName     = 0x08;
        public const int TimerRawNameSize = 0x10;

        public const int StatusSize        = 0x3C;
        public const int StatusTimeStamp   = 0x08;
        public const int StatusRawName     = 0x16;
        public const int StatusRawNameSize = 0x10;
    }

    public static class Airship
    {
        public const int TimerSize        = 0x24;
        public const int TimerTimeStamp   = 0x00;
        public const int TimerRawName     = 0x06;
        public const int TimerRawNameSize = 0x10;

        public const int StatusSize        = 0x24;
        public const int StatusTimeStamp   = 0x08;
        public const int StatusRawName     = 0x10;
        public const int StatusRawNameSize = 0x10;
    }

    public static class Retainer
    {
        public const int Size             = 0x48;
        public const int RetainerId       = 0x00;
        public const int Name             = 0x08;
        public const int NameSize         = 0x20;
        public const int ClassJob         = 0x29;
        public const int Gil              = 0x2C;
        public const int VentureId        = 0x38;
        public const int VentureTimestamp = 0x3C;

        public const int ContainerCount = 10;
        public const int ContainerData  = 0xC;
    }

    public static class Squadrons
    {
        public const int MissionEnd  = 0x00;
        public const int TrainingEnd = 0x04;
        public const int MissionId   = 0x08;
        public const int TrainingId  = 0x0A;
        public const int NewRecruits = 0x0C;
    }

    public static class FreeCompany
    {
        public const int FreeCompanyModuleVfunc = 34;
        public const int DataOffset             = 0x19E0;
    }
}

public static class Signatures
{
    public const string GoldSaucerData =
        "?? 89 ?? ?? ?? 57 48 83 ?? ?? 48 8B ?? ?? ?? ?? ?? 48 33 ?? ?? 89 ?? ?? ?? 48 8B ?? ?? ?? ?? ?? 48 8B ?? E8 ?? ?? ?? ?? 48 8B ?? 4C ?? ?? ?? ?? ?? ?? ?? ?? ?? 48 8B ?? BA B2";

    public const string SquadronContainer = "8B 3D ?? ?? ?? ?? 8B D8 3B F8";

    public const string RetainerContainer = "48 8B E9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 4E";

    public const string MapContainer = "48 83 ?? ?? ?? C7 ?? ?? ?? ?? ?? ?? ?? ?? ?? 45";

    public const string LeveAllowances = "88 05 ?? ?? ?? ?? 0F B7 41 06";

    public const string PositionInfo = "40 ?? 48 83 ?? ?? 33 DB 48 39 ?? ?? ?? ?? ?? 75 ?? 45";

    public const string AirshipTimers =
        "E8 ?? ?? ?? ?? 33 D2 48 8D 4C 24 ?? 41 B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 54 24 ?? 48 8B CB E8 ?? ?? ?? ?? 48 8B 0D";

    public const string AirshipStatus =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8D 99 ?? ?? ?? ?? C6 81";

    public const string SubmersibleTimers = "E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? E8 ?? ?? ?? ?? 84 C0 75";

    public const string SubmersibleStatus = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 0F 10 02 4C 8D 81";
}
