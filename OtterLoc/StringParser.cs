using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;

namespace OtterLoc;

public sealed class StringParser : ILocFilter
{
    private readonly Func<string, IList<string>> _func;

    public StringParser(Func<string, IList<string>> func)
        => _func = func;

    public string Name
        => "StringParser";

    public bool Matches(SeString seString)
        => Filter(seString).Count > 0;

    public bool Matches(string s)
        => Filter(s).Count > 0;

    public IList<string> Filter(SeString seString)
        => _func(seString.TextValue);

    public IList<string> Filter(string s)
        => _func(s);

    public static StringParser FromRange(ClientLanguage lang, Range en, Range fr, Range jp, Range de)
    {
        var range = lang switch
        {
            ClientLanguage.Japanese => jp,
            ClientLanguage.English  => en,
            ClientLanguage.German   => de,
            ClientLanguage.French   => fr,
            _                       => en,
        };

        IList<string> Func(string s)
        {
            var end = (uint)(range.End.IsFromEnd ? s.Length - range.End.Value : range.End.Value);
            if (end >= (uint)s.Length)
                return Array.Empty<string>();

            var start = (uint)(range.Start.IsFromEnd ? s.Length - range.Start.Value : range.Start.Value);
            if (start >= end)
                return Array.Empty<string>();

            return new[]
            {
                s.Substring((int)start, (int)end),
            };
        }

        return new StringParser(Func);
    }

    public static StringParser FromRegex(ClientLanguage lang, Regex en, Regex fr, Regex jp, Regex de, params string[] captureNames)
    {
        var regex = lang switch
        {
            ClientLanguage.Japanese => jp,
            ClientLanguage.English  => en,
            ClientLanguage.German   => de,
            ClientLanguage.French   => fr,
            _                       => en,
        };

        IList<string> Func(string se)
        {
            var match = regex.Match(se);
            return !match.Success
                ? Array.Empty<string>()
                : captureNames.Select(s => match.Groups[s].Value).ToArray();
        }

        return new StringParser(Func);
    }
}
