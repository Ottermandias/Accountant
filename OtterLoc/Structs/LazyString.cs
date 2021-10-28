using System;

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
}
