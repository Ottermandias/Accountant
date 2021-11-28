using System;
using System.Linq;

namespace Accountant.Classes;

public struct MiniCactpot
{
    public const int      MaxTickets = 3;
    public const int      ResetHour  = 15;
    public       DateTime LastUpdate = DateTime.MinValue;
    public       byte     Tickets    = 0;

    public DateTime NextReset()
        => NextReset(LastUpdate);

    public static DateTime NextReset(DateTime time)
        => new DateTime(time.Year, time.Month, time.Day, ResetHour, 0, 0, DateTimeKind.Utc).AddDays(time.Hour < ResetHour ? 0 : 1);
}

public struct JumboCactpot
{
    public const int    MaxTickets    = 3;
    public const ushort InvalidTicket = ushort.MaxValue;

    public DateTime LastUpdate;
    public ushort[] Tickets;

    public JumboCactpot()
    {
        LastUpdate = DateTime.MinValue;
        Tickets    = Enumerable.Repeat(InvalidTicket, MaxTickets).ToArray();
    }

    public bool EqualTickets(JumboCactpot rhs)
    {
        for (var i = 0; i < MaxTickets; ++i)
        {
            if (Tickets[i] != rhs.Tickets[i])
                return false;
        }

        return true;
    }

    public void ClearTickets()
    {
        for (var i = 0; i < MaxTickets; ++i)
            Tickets[i] = InvalidTicket;
    }

    public bool IsFull()
        => Tickets[MaxTickets - 1] != InvalidTicket;

    public bool IsEmpty()
        => Tickets[0] == InvalidTicket;

    public int Count()
    {
        for (var i = 0; i < MaxTickets; ++i)
        {
            if (Tickets[i] == InvalidTicket)
                return i;
        }

        return MaxTickets;
    }

    public void SetFirstTicket(ushort ticket)
    {
        for (var i = 0; i < MaxTickets; ++i)
        {
            if (Tickets[i] == InvalidTicket)
            {
                Tickets[i] = ticket;
                return;
            }
        }
    }

    public void ClearFirstTicket()
    {
        for (var i = 0; i < MaxTickets - 1; ++i)
            Tickets[i] = Tickets[i + 1];
        Tickets[MaxTickets - 1] = InvalidTicket;
    }

    public DateTime NextReset(ushort worldId)
        => NextReset(LastUpdate, worldId);

    public static DateTime NextReset(DateTime time, ushort worldId)
    {
        var hour = Accountant.GameData.GetJumboCactpotResetHour(worldId);

        var dayOffset = DayOfWeek.Saturday - time.DayOfWeek;
        var ret       = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(hour + 24 * dayOffset);
        return ret < time ? ret.AddDays(7) : ret;
    }
}
