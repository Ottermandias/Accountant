using System.Linq;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace OtterLoc;

public sealed class StringMatcherLetters : ILocMatcher
{
    public StringMatcherLetters(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name.Where(char.IsLetter))
            sb.Append(c);
        Name = sb.ToString();
    }

    public string Name { get; }

    public bool Matches(string s)
    {
        var homeIdx = 0;
        foreach (var c in s.Where(char.IsLetter))
        {
            if (c != Name[homeIdx++])
                return false;
        }

        return homeIdx == Name.Length;
    }

    public bool Matches(SeString s)
        => Matches(s.TextValue);
}
