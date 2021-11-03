using System;
using System.Linq;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;

namespace OtterLoc;

public sealed class SeStringMatcher : ILocMatcher
{
    private readonly Func<SeString, bool> _func;

    public SeStringMatcher(Func<SeString, bool> func)
        => _func = func;

    public string Name
        => "Func";

    public bool Matches(string s)
        => throw new NotImplementedException();

    public bool Matches(SeString s)
        => _func(s);

    public static SeStringMatcher SinglePayloadComparer(ClientLanguage lang, SeString s, Index idxEn, Index idxFr, Index idxJp, Index idxDe)
    {
        var idx = lang switch
        {
            ClientLanguage.Japanese => idxJp,
            ClientLanguage.German   => idxDe,
            ClientLanguage.French   => idxFr,
            ClientLanguage.English  => idxEn,
            _                       => idxEn,
        };
        var payload = s.Payloads[idx];
        var bytes   = payload.Encode().Where(c => c != '\r' && c != '\n').ToArray();
        var type    = payload.Type;
        bool Func(SeString x)
        {
            var index = (uint) (idx.IsFromEnd ? x.Payloads.Count - idx.Value : idx.Value);
            if (index >= x.Payloads.Count)
                return false;
            var p = x.Payloads[(int) index];
            return p.Type == type && bytes.SequenceEqual(p.Encode().Where(c => c != '\r' && c != '\n'));
        }

        return new SeStringMatcher(Func);
    }
}