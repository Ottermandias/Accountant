using System;
using Accountant.Enums;
using Accountant.Manager;
using Dalamud.Game.Text;
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
        public  int      Available { get; private set; }
        public  int      Completed { get; private set; }
        public  int      Sent      { get; private set; }
        private int      _limited;
        private int      _total;

        public string GetHeader(SeIconChar icon)
            => $"{icon.ToIconChar()}{Completed}|{Available}|{Sent}";

        public string GetHeader(StringId s)
            => $"{s.Value()}: {Completed} | {Available} | {Sent}";

        public ColorId GetColorText()
        {
            if (_limited == _total)
                return ColorId.DisabledText;

            var diff = _total - _limited;
            if (Completed == diff)
                return ColorId.TextObjectsHome;

            if (Sent == diff)
                return ColorId.TextObjectsAway;

            return ColorId.TextObjectsMixed;
        }

        public ColorId GetColorHeader()
            => GetColorText().TextToHeader();

        public DateTime GetTime()
            => Sent == _total - _limited ? _timeForFirst : _timeForAll;

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
                Available           = lhs.Available + rhs.Available,
                Completed           = lhs.Completed + rhs.Completed,
                Sent                = lhs.Sent + rhs.Sent,
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
                ++Available;
                return ObjectStatus.Available;
            }

            if (dateTime <= now)
            {
                _timeForFirst = DateTime.MinValue;
                ++Completed;
                return ObjectStatus.Completed;
            }

            if (_timeForFirst > dateTime)
                _timeForFirst = dateTime;
            if (_actualTimeForFirst > dateTime)
                _actualTimeForFirst = dateTime;
            if (_timeForAll < dateTime)
                _timeForAll = dateTime;
            ++Sent;
            return ObjectStatus.Sent;
        }

        private bool LimitReached(int maxSent)
            => Sent + Completed >= maxSent;

        public bool VerifyLimit(int maxSent)
        {
            if (!LimitReached(maxSent))
                return false;

            _timeForFirst =  _actualTimeForFirst;
            _limited      += Available;
            Available     =  0;
            return true;
        }
    }
}
