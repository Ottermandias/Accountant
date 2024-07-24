using System;

namespace Accountant.Internal;

public static class Helpers
{
    public static DateTime DateFromTimeStamp(uint timeStamp)
    {
        const long timeFromEpoch = 62135596800;

        return timeStamp == 0u
            ? DateTime.MinValue
            : new DateTime((timeStamp + timeFromEpoch) * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
    }

    public static TimeSpan TimeLeftFromNow(DateTime time)
    {
        return time > DateTime.UtcNow
            ? time - DateTime.UtcNow
            : TimeSpan.Zero;
    }

    public static string FormatTimeSpan(TimeSpan timeLeft)
    {
        return string.Format(timeLeft.TotalHours >= 1 ? "{0:D2}:{1:D2}:{2:D2}" : "{1:D2}:{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
    }
}
