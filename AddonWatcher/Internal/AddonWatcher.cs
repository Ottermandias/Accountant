using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace AddonWatcher.Internal;

internal class AddonWatcher : IAddonWatcher
{
    public const int CurrentVersion = 1;

    private static AddonWatcherBase? _base;
    private static uint              _subscribers;

    public int Version
        => CurrentVersion;

    public bool Valid { get; private set; } = true;

    public AddonWatcher(IPluginLog log, IGameGui gui, ISigScanner sigScanner, IGameInteropProvider interop)
    {
        _base ??= new AddonWatcherBase(log, gui, sigScanner, interop);
        ++_subscribers;
    }


    public void Dispose()
    {
        Valid = false;
        if (_subscribers == 0)
            return;

        --_subscribers;
        if (_subscribers == 0)
        {
            _base?.Dispose();
            _base = null;
        }
    }

    private AddonWatcherBase Base
        => Valid ? _base! : throw new InvalidOperationException("Accessing disposed AddonWatcher.");

    public void SubscribeYesnoSelected(ReceiveSelectYesnoDelegate del)
        => Base.SubscribeYesnoSelected(del);

    public void SubscribeStringSelected(ReceiveSelectStringDelegate del)
        => Base.SubscribeStringSelected(del);

    public void SubscribeSelectStringSetup(SelectStringSetupDelegate del)
        => Base.SubscribeSelectStringSetup(del);

    public void SubscribeSelectYesnoSetup(SelectYesnoSetupDelegate del)
        => Base.SubscribeSelectYesnoSetup(del);

    public void SubscribeJournalResultSetup(JournalResultSetupDelegate del)
        => Base.SubscribeJournalResultSetup(del);

    public void SubscribeLotteryWeeklyRewardListSetup(LotteryWeeklyRewardListSetupDelegate del)
        => Base.SubscribeLotteryWeeklyRewardListSetup(del);

    public void SubscribeTalkUpdate(TalkUpdateDelegate del)
        => Base.SubscribeTalkUpdate(del);


    public void UnsubscribeYesnoSelected(ReceiveSelectYesnoDelegate del)
        => Base.UnsubscribeYesnoSelected(del);

    public void UnsubscribeStringSelected(ReceiveSelectStringDelegate del)
        => Base.UnsubscribeStringSelected(del);

    public void UnsubscribeSelectStringSetup(SelectStringSetupDelegate del)
        => Base.UnsubscribeSelectStringSetup(del);

    public void UnsubscribeSelectYesnoSetup(SelectYesnoSetupDelegate del)
        => Base.UnsubscribeSelectYesnoSetup(del);

    public void UnsubscribeJournalResultSetup(JournalResultSetupDelegate del)
        => Base.UnsubscribeJournalResultSetup(del);

    public void UnsubscribeLotteryWeeklyRewardListSetup(LotteryWeeklyRewardListSetupDelegate del)
        => Base.UnsubscribeLotteryWeeklyRewardListSetup(del);

    public void UnsubscribeTalkUpdate(TalkUpdateDelegate del)
        => Base.UnsubscribeTalkUpdate(del);


    public void SubscribeOnceYesnoSelected(ReceiveSelectYesnoDelegate del)
        => Base.SubscribeOnceYesnoSelected(del);

    public void SubscribeOnceStringSelected(ReceiveSelectStringDelegate del)
        => Base.SubscribeOnceStringSelected(del);

    public void SubscribeOnceSelectStringSetup(SelectStringSetupDelegate del)
        => Base.SubscribeOnceSelectStringSetup(del);

    public void SubscribeOnceSelectYesnoSetup(SelectYesnoSetupDelegate del)
        => Base.SubscribeOnceSelectYesnoSetup(del);

    public void SubscribeOnceJournalResultSetup(JournalResultSetupDelegate del)
        => Base.SubscribeOnceJournalResultSetup(del);

    public void SubscribeOnceLotteryWeeklyRewardListSetup(LotteryWeeklyRewardListSetupDelegate del)
        => Base.SubscribeOnceLotteryWeeklyRewardListSetup(del);

    public void SubscribeOnceTalkUpdate(TalkUpdateDelegate del)
        => Base.SubscribeOnceTalkUpdate(del);
}
