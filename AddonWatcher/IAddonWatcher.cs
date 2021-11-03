using System;
using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;

namespace AddonWatcher;

public delegate void ReceiveSelectYesnoDelegate(IntPtr unit, bool yesOrNo, SeString buttonText, SeString descriptionText);
public delegate void ReceiveSelectStringDelegate(IntPtr unit, int which, SeString buttonText, SeString descriptionText);
public delegate void SelectStringSetupDelegate(IntPtr unit, SeString description, SeString[] options);
public delegate void SelectYesnoSetupDelegate(IntPtr unit, SeString description, SeString yesButton, SeString noButton);
public delegate void JournalResultSetupDelegate(IntPtr unit, SeString questName);
public delegate void TalkUpdateDelegate(IntPtr unit, SeString text, SeString speaker);


public static class AddonWatcherFactory
{
    public static IAddonWatcher Create(GameGui gui, SigScanner sigScanner)
        => new Internal.AddonWatcher(gui, sigScanner);
}


public interface IAddonWatcher : IDisposable
{
    public int  Version { get; }
    public bool Valid   { get; }

    public void SubscribeYesnoSelected(ReceiveSelectYesnoDelegate del);
    public void SubscribeStringSelected(ReceiveSelectStringDelegate del);
    public void SubscribeSelectStringSetup(SelectStringSetupDelegate del);
    public void SubscribeSelectYesnoSetup(SelectYesnoSetupDelegate del);
    public void SubscribeJournalResultSetup(JournalResultSetupDelegate del);
    public void SubscribeTalkUpdate(TalkUpdateDelegate del);


    public void UnsubscribeYesnoSelected(ReceiveSelectYesnoDelegate del);
    public void UnsubscribeStringSelected(ReceiveSelectStringDelegate del);
    public void UnsubscribeSelectStringSetup(SelectStringSetupDelegate del);
    public void UnsubscribeSelectYesnoSetup(SelectYesnoSetupDelegate del);
    public void UnsubscribeJournalResultSetup(JournalResultSetupDelegate del);
    public void UnsubscribeTalkUpdate(TalkUpdateDelegate del);


    public void SubscribeOnceYesnoSelected(ReceiveSelectYesnoDelegate del);
    public void SubscribeOnceStringSelected(ReceiveSelectStringDelegate del);
    public void SubscribeOnceSelectStringSetup(SelectStringSetupDelegate del);
    public void SubscribeOnceSelectYesnoSetup(SelectYesnoSetupDelegate del);
    public void SubscribeOnceJournalResultSetup(JournalResultSetupDelegate del);
    public void SubscribeOnceTalkUpdate(TalkUpdateDelegate del);
}
