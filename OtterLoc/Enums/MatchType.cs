namespace OtterLoc.Enums;

public enum MatchType : byte
{
    Equal,
    Contains,
    StartsWith,
    EndsWith,
    CiEqual,
    CiContains,
    CiStartsWith,
    CiEndsWith,
    RegexFull,
    RegexPartial,
}
