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
    public const string GoldSaucerData = "48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B 0D ?? ?? ?? ?? 48 8B FA";
    public const string SquadronContainer = "8B 3D ?? ?? ?? ?? 8B D8 3B F8";
    public const string PositionInfo = "48 8B 05 ?? ?? ?? ?? 0F 83";


    public const string AirshipTimers = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B D9 48 8B FA 48 8B 49 40";
    public const string AirshipStatus = "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC ?? 48 8D B1";

    public const string SubmersibleTimers = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B D9 48 8B FA 48 8B 49 48";
    public const string SubmersibleStatus = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 0F 10 02 4C 8D 81";
}