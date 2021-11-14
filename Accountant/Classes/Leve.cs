using System;

namespace Accountant.Classes;

public struct Leve
{
    public const int AllowanceCap   = 100;
    public const int AllowanceError = 97;

    public int      Allowances;
    public DateTime LastUpdate;

    public int CurrentAllowances(DateTime now)
    {
        if (Allowances == 100)
            return 100;

        var timeSpan = Round(now) - LastUpdate;
        return Math.Min(100, Allowances + 3 * (int)(timeSpan.TotalHours / 12));
    }

    public static DateTime Round(DateTime date)
        => new(date.Year, date.Month, date.Day, date.Hour >= 12 ? 12 : 0, 0, 0, DateTimeKind.Utc);
}
