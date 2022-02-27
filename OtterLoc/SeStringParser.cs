using System;
using System.Collections.Generic;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace OtterLoc;

public sealed class SeStringParser : ILocFilter
{
    private readonly Func<SeString, IList<string>> _func;

    public string Name
        => "Filter";

    public SeStringParser(Func<SeString, IList<string>> func)
        => _func = func;

    public bool Matches(SeString seString)
        => Filter(seString).Count > 0;

    public bool Matches(string s)
        => throw new NotImplementedException();

    public IList<string> Filter(SeString seString)
        => _func(seString);

    public IList<string> Filter(string s)
        => throw new NotImplementedException();

    public static SeStringParser SpecificPayload(ClientLanguage lang, Index en, Index fr, Index jp, Index de)
    {
        var idx = lang switch
        {
            ClientLanguage.Japanese => jp,
            ClientLanguage.English  => en,
            ClientLanguage.German   => de,
            ClientLanguage.French   => fr,
            _                       => en,
        };

        IList<string> Func(SeString se)
        {
            var index = (uint)(idx.IsFromEnd ? se.Payloads.Count - idx.Value : idx.Value);
            if (index >= se.Payloads.Count)
                return Array.Empty<string>();

            return se.Payloads[(int)index] is not TextPayload text
                ? Array.Empty<string>()
                : new[]
                {
                    text.Text!,
                };
        }

        return new SeStringParser(Func);
    }
}
