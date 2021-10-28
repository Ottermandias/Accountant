using System;
using Dalamud.Game.Text.SeStringHandling;

namespace AddonWatcher.Internal;

internal partial class AddonWatcherBase
{
    public void SubscribeYesnoSelected(ReceiveSelectYesnoDelegate del)
    {
        YesnoSelected += del;
        if (!SelectYesNoHook!.IsEnabled)
            SelectYesNoHook.Enable();
    }

    public void UnsubscribeYesnoSelected(ReceiveSelectYesnoDelegate del)
    {
        YesnoSelected -= del;
        if (YesnoSelected == null)
            SelectYesNoHook!.Disable();
    }

    public void SubscribeOnceYesnoSelected(ReceiveSelectYesnoDelegate del)
    {
        void NewDel(IntPtr unit, bool yesOrNo, SeString buttonText, SeString descriptionText)
        {
            del(unit, yesOrNo, buttonText, descriptionText);
            UnsubscribeYesnoSelected(NewDel);
        }

        SubscribeYesnoSelected(NewDel);
    }


    public void SubscribeStringSelected(ReceiveSelectStringDelegate del)
    {
        StringSelected += del;
        if (!SelectStringHook!.IsEnabled)
            SelectStringHook.Enable();
    }

    public void UnsubscribeStringSelected(ReceiveSelectStringDelegate del)
    {
        StringSelected -= del;
        if (StringSelected == null)
            SelectStringHook!.Disable();
    }

    public void SubscribeOnceStringSelected(ReceiveSelectStringDelegate del)
    {
        void NewDel(IntPtr unit, int which, SeString buttonText, SeString descriptionText)
        {
            del(unit, which, buttonText, descriptionText);
            UnsubscribeStringSelected(NewDel);
        }

        SubscribeStringSelected(NewDel);
    }


    public void SubscribeSelectStringSetup(SelectStringSetupDelegate del)
    {
        SelectStringSetup += del;
        if (SelectStringSetupHook!.IsEnabled)
            SelectStringSetupHook.Enable();
    }

    public void UnsubscribeSelectStringSetup(SelectStringSetupDelegate del)
    {
        SelectStringSetup -= del;
        if (SelectStringSetup == null)
            SelectStringSetupHook!.Disable();
    }

    public void SubscribeOnceSelectStringSetup(SelectStringSetupDelegate del)
    {
        void NewDel(IntPtr unit, SeString description, SeString[] options)
        {
            del(unit, description, options);
            UnsubscribeSelectStringSetup(NewDel);
        }

        SubscribeSelectStringSetup(NewDel);
    }


    public void SubscribeSelectYesnoSetup(SelectYesnoSetupDelegate del)
    {
        SelectYesnoSetup += del;
        if (SelectYesnoSetupHook!.IsEnabled)
            SelectYesnoSetupHook.Enable();
    }

    public void UnsubscribeSelectYesnoSetup(SelectYesnoSetupDelegate del)
    {
        SelectYesnoSetup -= del;
        if (SelectYesnoSetup == null)
            SelectYesnoSetupHook!.Disable();
    }

    public void SubscribeOnceSelectYesnoSetup(SelectYesnoSetupDelegate del)
    {
        void NewDel(IntPtr unit, SeString description, SeString yesButton, SeString noButton)
        {
            del(unit, description, yesButton, noButton);
            UnsubscribeSelectYesnoSetup(NewDel);
        }

        SubscribeSelectYesnoSetup(NewDel);
    }


    public void SubscribeJournalResultSetup(JournalResultSetupDelegate del)
    {
        JournalResultSetup += del;
        if (JournalResultSetupHook!.IsEnabled)
            JournalResultSetupHook.Enable();
    }

    public void UnsubscribeJournalResultSetup(JournalResultSetupDelegate del)
    {
        JournalResultSetup -= del;
        if (JournalResultSetup == null)
            JournalResultSetupHook!.Disable();
    }

    public void SubscribeOnceJournalResultSetup(JournalResultSetupDelegate del)
    {
        void NewDel(IntPtr unit, SeString questName)
        {
            del(unit, questName);
            UnsubscribeJournalResultSetup(NewDel);
        }

        SubscribeJournalResultSetup(NewDel);
    }


    public void SubscribeTalkUpdate(TalkUpdateDelegate del)
    {
        TalkUpdated += del;
        if (!TalkUpdateHook!.IsEnabled)
            TalkUpdateHook.Enable();
    }

    public void UnsubscribeTalkUpdate(TalkUpdateDelegate del)
    {
        TalkUpdated -= del;
        if (TalkUpdated == null)
            TalkUpdateHook!.Disable();
    }

    public void SubscribeOnceTalkUpdate(TalkUpdateDelegate del)
    {
        void NewDel(IntPtr unit, SeString text, SeString speaker)
        {
            del(unit, text, speaker);
            UnsubscribeTalkUpdate(NewDel);
        }

        SubscribeTalkUpdate(NewDel);
    }
}
