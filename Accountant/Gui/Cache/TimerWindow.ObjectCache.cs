using System;
using Accountant.Manager;

namespace Accountant.Gui;

public partial class TimerWindow
{
    private class ObjectCache : BaseCache
    {
        protected int      AvailableObjects = 0;
        protected int      CompletedObjects = 0;
        protected int      SentObjects      = 0;
        protected int      LimitedObjects   = 0;
        protected int      TotalObjects     = 0;
        protected DateTime TimeForFirst     = DateTime.MaxValue;
        protected DateTime TimeForAll       = DateTime.MinValue;

        protected int      CurrentAvailableObjects   = 0;
        protected int      CurrentCompletedObjects   = 0;
        protected int      CurrentSentObjects        = 0;
        protected int      CurrentLimitedObjects     = 0;
        protected DateTime CurrentTimeForFirst       = DateTime.MaxValue;
        protected DateTime CurrentActualTimeForFirst = DateTime.MaxValue;
        protected DateTime CurrentTimeForAll         = DateTime.MinValue;

        public DateTime GlobalTime  = DateTime.MinValue;
        public ColorId  GlobalColor = 0;
        public string   Header      = string.Empty;

        protected void ResetCurrent()
        {
            CurrentAvailableObjects   = 0;
            CurrentCompletedObjects   = 0;
            CurrentSentObjects        = 0;
            CurrentLimitedObjects     = 0;
            CurrentTimeForFirst       = DateTime.MaxValue;
            CurrentActualTimeForFirst = DateTime.MaxValue;
            CurrentTimeForAll         = DateTime.MinValue;
        }

        protected void AddCurrent()
        {
            AvailableObjects += CurrentAvailableObjects;
            CompletedObjects += CurrentCompletedObjects;
            SentObjects      += CurrentSentObjects;
            LimitedObjects   += CurrentLimitedObjects;
            TotalObjects     =  AvailableObjects + CompletedObjects + SentObjects + LimitedObjects;
            if (TimeForAll < CurrentTimeForAll)
                TimeForAll = CurrentTimeForAll;
            if (CurrentLimitedObjects > 0)
            {
                if (TimeForFirst > CurrentActualTimeForFirst)
                    TimeForFirst = CurrentActualTimeForFirst;
            }
            else
            {
                if (TimeForFirst > CurrentTimeForFirst)
                    TimeForFirst = CurrentTimeForFirst;
            }
        }

        protected ObjectCache(TimerWindow window, TimerManager manager)
            : base(window, manager)
        { }

        protected override void UpdateInternal()
        {
            AvailableObjects = 0;
            CompletedObjects = 0;
            SentObjects      = 0;
            LimitedObjects   = 0;
            TimeForFirst     = DateTime.MaxValue;
            TimeForAll       = DateTime.MinValue;
        }

        protected string? GetDisplayInfo(DateTime displayTime)
        {
            UpdateNextChange(displayTime);
            if (displayTime == DateTime.MinValue)
            {
                CurrentTimeForFirst = DateTime.MinValue;
                ++CurrentAvailableObjects;
                return Window._availableString;
            }

            if (displayTime <= Now)
            {
                CurrentTimeForFirst = DateTime.MinValue;
                ++CurrentCompletedObjects;
                return Window._completedString;
            }

            if (CurrentTimeForFirst > displayTime)
                CurrentTimeForFirst = displayTime;
            if (CurrentTimeForAll < displayTime)
                CurrentTimeForAll = displayTime;
            if (CurrentActualTimeForFirst > displayTime)
                CurrentActualTimeForFirst = displayTime;
            ++CurrentSentObjects;
            return null;
        }
    }
}
