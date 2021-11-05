using System;
using AddonWatcher.SeFunctions;
using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Hooking;

namespace AddonWatcher.Internal;

internal partial class AddonWatcherBase : IDisposable
{
    private readonly GameGui _gui;

    internal SelectStringReceiveEvent SelectStringReceiveEvent;
    internal SelectYesnoReceiveEvent  SelectYesnoReceiveEvent;
    internal SelectStringOnSetup      SelectStringOnSetup;
    internal SelectYesnoOnSetup       SelectYesnoOnSetup;
    internal JournalResultOnSetup     JournalResultOnSetup;
    internal TalkOnUpdate             TalkOnUpdate;

    internal Hook<OnAddonReceiveEventDelegate>? SelectYesNoHook;
    internal Hook<OnAddonReceiveEventDelegate>? SelectStringHook;
    internal Hook<OnAddonSetupDelegate>?        SelectYesnoSetupHook;
    internal Hook<OnAddonSetupDelegate>?        SelectStringSetupHook;
    internal Hook<OnAddonSetupDelegate>?        JournalResultSetupHook;
    internal Hook<OnAddonUpdateDelegate>?       TalkUpdateHook;

    private event ReceiveSelectYesnoDelegate?  YesnoSelected;
    private event ReceiveSelectStringDelegate? StringSelected;
    private event SelectStringSetupDelegate?   SelectStringSetup;
    private event SelectYesnoSetupDelegate?    SelectYesnoSetup;
    private event JournalResultSetupDelegate?  JournalResultSetup;
    private event TalkUpdateDelegate?          TalkUpdated;

    public AddonWatcherBase(GameGui gui, SigScanner sigScanner)
    {
        _gui = gui;

        SelectStringReceiveEvent = new SelectStringReceiveEvent(sigScanner);
        SelectYesnoReceiveEvent  = new SelectYesnoReceiveEvent(sigScanner);
        SelectStringOnSetup      = new SelectStringOnSetup(sigScanner);
        SelectYesnoOnSetup       = new SelectYesnoOnSetup(sigScanner);
        JournalResultOnSetup     = new JournalResultOnSetup(sigScanner);
        TalkOnUpdate             = new TalkOnUpdate(sigScanner);

        SelectYesNoHook        = SelectYesnoReceiveEvent.CreateHook(SelectYesNoEventDetour, false);
        SelectStringHook       = SelectStringReceiveEvent.CreateHook(SelectStringEventDetour, false);
        SelectYesnoSetupHook   = SelectYesnoOnSetup.CreateHook(SelectYesnoOnSetupDetour, false);
        SelectStringSetupHook  = SelectStringOnSetup.CreateHook(SelectStringOnSetupDetour, false);
        JournalResultSetupHook = JournalResultOnSetup.CreateHook(JournalResultOnSetupDetour, false);
        TalkUpdateHook         = TalkOnUpdate.CreateHook(TalkUpdateDetour, false);
    }

    public void Dispose()
    {
        SelectYesNoHook?.Dispose();
        SelectStringHook?.Dispose();
        SelectYesnoSetupHook?.Dispose();
        SelectStringSetupHook?.Dispose();
        JournalResultSetupHook?.Dispose();
        TalkUpdateHook?.Dispose();
    }
}
