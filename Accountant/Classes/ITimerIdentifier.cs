namespace Accountant.Timers;

public interface ITimerIdentifier
{
    public string Name     { get; }
    public ushort ServerId { get; }
    public uint   IdentifierHash();
}
