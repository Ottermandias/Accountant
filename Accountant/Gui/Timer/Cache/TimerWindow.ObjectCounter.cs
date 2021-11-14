using System;
using Accountant.Enums;
using Accountant.Manager;
using OtterLoc.Structs;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    internal enum ObjectStatus
    {
        Available,
        Completed,
        Sent,
        Limited,
    }

    internal struct ObjectCounter
    {
        private DateTime _timeForFirst;
        private DateTime _timeForAll;
        private DateTime _actualTimeForFirst;
        private int      _available;
        private int      _completed;
        private int      _sent;
        private int      _limited;
        private int      _total;

        public string GetHeader(StringId s)
            => $"{s.Value()}: {_completed} | {_available} | {_sent}";

        public ColorId GetColorText()
        {
            if (_limited == _total)
                return ColorId.DisabledText;

            var diff = _total - _limited;
            if (_completed == diff)
                return ColorId.TextObjectsHome;

            if (_sent == diff)
                return ColorId.TextObjectsAway;

            return ColorId.TextObjectsMixed;
        }

        public ColorId GetColorHeader()
            => GetColorText().TextToHeader();

        public DateTime GetTime()
            => _sent == _total - _limited ? _timeForFirst : _timeForAll;

        public static ObjectCounter Create()
            => new()
            {
                _timeForFirst       = DateTime.MaxValue,
                _actualTimeForFirst = DateTime.MaxValue,
                _timeForAll         = DateTime.MinValue,
            };

        public static ObjectCounter operator +(ObjectCounter lhs, ObjectCounter rhs)
            => new()
            {
                _total              = lhs._total + rhs._total,
                _available          = lhs._available + rhs._available,
                _completed          = lhs._completed + rhs._completed,
                _sent               = lhs._sent + rhs._sent,
                _limited            = lhs._limited + rhs._limited,
                _timeForFirst       = lhs._timeForFirst < rhs._timeForFirst ? lhs._timeForFirst : rhs._timeForFirst,
                _timeForAll         = lhs._timeForAll > rhs._timeForAll ? lhs._timeForAll : rhs._timeForAll,
                _actualTimeForFirst = lhs._actualTimeForFirst < rhs._actualTimeForFirst ? lhs._actualTimeForFirst : rhs._actualTimeForFirst,
            };

        public ObjectStatus Add(DateTime dateTime, DateTime now, int maxSent = int.MaxValue)
        {
            ++_total;
            if (dateTime == DateTime.MinValue)
            {
                if (LimitReached(maxSent))
                {
                    ++_limited;
                    return ObjectStatus.Limited;
                }

                _timeForFirst = DateTime.MinValue;
                ++_available;
                return ObjectStatus.Available;
            }

            if (dateTime <= now)
            {
                _timeForFirst = DateTime.MinValue;
                ++_completed;
                return ObjectStatus.Completed;
            }

            if (_timeForFirst > dateTime)
                _timeForFirst = dateTime;
            if (_actualTimeForFirst > dateTime)
                _actualTimeForFirst = dateTime;
            if (_timeForAll < dateTime)
                _timeForAll = dateTime;
            ++_sent;
            return ObjectStatus.Sent;
        }

        private bool LimitReached(int maxSent)
            => _sent + _completed >= maxSent;

        public bool VerifyLimit(int maxSent)
        {
            if (!LimitReached(maxSent))
                return false;

            _timeForFirst =  _actualTimeForFirst;
            _limited      += _available;
            _available    =  0;
            return true;
        }
    }
}
