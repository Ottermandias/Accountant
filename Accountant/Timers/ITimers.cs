using System.Collections.Generic;

namespace Accountant.Timers;

public interface ITimers<TIdent, TInfo>
    where TIdent : struct, ITimerIdentifier
{
    public IReadOnlyDictionary<TIdent, TInfo> Data { get; }
    public event TimerChange?                 Changed;

    public void Save(TIdent ident, TInfo info);
    public void Save(TIdent ident);
    public void DeleteFile(TIdent ident);
    public void Set(TIdent ident, TInfo info);
}
