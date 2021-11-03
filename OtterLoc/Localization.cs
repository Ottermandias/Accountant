using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterLoc.Enums;
using OtterLoc.Structs;

namespace OtterLoc;

public interface ILocObject
{
    public string Name { get; }
}

public interface ILocMatcher : ILocObject
{
    public bool Matches(SeString seString);
    public bool Matches(string s);
}

public interface ILocFilter : ILocMatcher
{
    public IList<string> Filter(SeString seString);
    public IList<string> Filter(string s);
}

public static class LocalizationDict<T> where T : Enum
{
    private sealed class LocEmpty : ILocObject
    {
        public string Name
            => string.Empty;
    }

    private static readonly int              NumValues    = Enum.GetValues(typeof(T)).Length;
    private static readonly List<ILocObject> InternalData = Enumerable.Repeat((ILocObject)new LocEmpty(), NumValues).ToList();

    public static bool Register(T key, ILocObject obj)
    {
        var idx = RangeCheck(key);
        InternalData[idx] = obj;
        return true;
    }

    public static bool Register(T key, string s)
        => Register(key, new LocName(s));

    public static bool RegisterName(T key, ClientLanguage lang, string en, string jp, string de, string fr)
        => lang switch
        {
            ClientLanguage.Japanese => Register(key, jp),
            ClientLanguage.German   => Register(key, de),
            ClientLanguage.French   => Register(key, fr),
            _                       => Register(key, en),
        };

    public static bool RegisterName(T key, ClientLanguage lang, SeString se, int payloadEn, int payloadJp, int payloadDe, int payloadFr)
        => lang switch
        {
            ClientLanguage.Japanese => Register(key, (se.Payloads[payloadJp] as TextPayload)?.Text.Trim() ?? string.Empty),
            ClientLanguage.German   => Register(key, (se.Payloads[payloadDe] as TextPayload)?.Text.Trim() ?? string.Empty),
            ClientLanguage.French   => Register(key, (se.Payloads[payloadFr] as TextPayload)?.Text.Trim() ?? string.Empty),
            _                       => Register(key, (se.Payloads[payloadEn] as TextPayload)?.Text.Trim() ?? string.Empty),
        };

    public static bool RegisterComparer(T key, string s, MatchType type = MatchType.Equal)
        => Register(key, new StringMatcher(s, type));

    public static bool RegisterComparer(T key, CompareString s)
        => Register(key, new StringMatcher(s));

    public static bool Register(T key, Func<SeString, bool> matcher)
        => Register(key, new SeStringMatcher(matcher));

    public static bool Register(T key, Func<SeString, IList<string>> filter)
        => Register(key, new SeStringParser(filter));

    private static int RangeCheck(T key)
    {
        var idx = Unsafe.As<T, int>(ref key);
        if (idx >= NumValues)
            throw new ArgumentOutOfRangeException($"{key} is not defined.");

        return idx;
    }

    public static ILocObject Get(T key)
    {
        var idx = RangeCheck(key);
        var loc = InternalData[idx];
        if (loc is LocEmpty)
            throw new InvalidOperationException($"{key} is not initialized.");

        return loc;
    }

    public static string GetName(T key)
    {
        var loc = Get(key);
        return loc.Name;
    }
}
