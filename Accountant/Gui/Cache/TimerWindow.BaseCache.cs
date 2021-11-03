using System;
using System.Collections.Generic;
using Accountant.Manager;

namespace Accountant.Gui;

public partial class TimerWindow
{
    private class BaseCache
    {
        protected readonly TimerWindow       Window;
        protected readonly TimerManager      Manager;
        private readonly   HashSet<string>   _seenNames = new();
        public readonly    List<CacheObject> Objects    = new();

        protected DateTime Now;
        private   DateTime _nextChange = DateTime.MinValue;

        protected BaseCache(TimerWindow window, TimerManager manager)
        {
            Window  = window;
            Manager = manager;
        }

        protected void UpdateNextChange(DateTime time)
        {
            if (time >= _nextChange)
                return;

            if (time > Now)
                _nextChange = time;
            else if (time == Now)
                _nextChange = Now.AddMilliseconds(100);
        }

        protected string GetName(string name, uint serverId)
        {
            if (_seenNames.Add(name))
                return name;

            var server = Accountant.GameData.GetWorldName(serverId);
            return $"{name} ({server})";
        }

        public void Update(DateTime now)
        {
            if (now <= _nextChange)
                return;

            Now         = now;
            _nextChange = DateTime.MaxValue;
            _seenNames.Clear();
            Objects.Clear();

            UpdateInternal();
        }

        protected virtual void UpdateInternal()
        { }

        protected void Resetter()
            => _nextChange = DateTime.UtcNow;
    }
}
