namespace OtterLoc;

public sealed class LocName : ILocObject
{
    public string Name { get; }

    public LocName(string name)
        => Name = name;
}
