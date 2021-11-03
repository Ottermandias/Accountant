using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace OtterLoc.Structs;

public readonly struct LazyString<T> where T : Enum
{
    public T Id { get; }

    public LazyString(T id)
        => Id = id;

    public static implicit operator string(LazyString<T> s)
        => LocalizationDict<T>.GetName(s.Id);

    public static explicit operator LazyString<T>(T s)
        => new(s);

    public override string ToString()
        => this;
}

public static class StringIdExtensions
{
    public static LazyString<T> Value<T>(this T s) where T : Enum
        => new(s);

    public static ILocObject Object<T>(this T s) where T : Enum
        => LocalizationDict<T>.Get(s);

    public static IList<string> Filter<T>(this T s, SeString ss) where T : Enum
        => ((ILocFilter)LocalizationDict<T>.Get(s)).Filter(ss);

    public static bool Match<T>(this T s, SeString ss) where T : Enum
        => ((ILocMatcher)LocalizationDict<T>.Get(s)).Matches(ss);
}
