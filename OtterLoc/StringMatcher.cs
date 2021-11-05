using Dalamud.Game.Text.SeStringHandling;
using OtterLoc.Enums;
using OtterLoc.Structs;

namespace OtterLoc;

public sealed class StringMatcher : ILocMatcher
{
    private readonly CompareString _string;

    public StringMatcher(CompareString s)
        => _string = s;

    public StringMatcher(string name, MatchType type)
        => _string = new CompareString(name, type);

    public string Name
        => _string.Text;

    public bool Matches(string s)
        => _string.Matches(s);

    public bool Matches(SeString s)
        => Matches(s.TextValue);
}
