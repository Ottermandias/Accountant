using System;

namespace Accountant.Classes;

public struct Tribe
{
    public const int AllowanceCap = 12;

    public int      Allowances;
    public DateTime LastUpdate;

    public int CurrentAllowances(DateTime now)
        => Allowances == AllowanceCap || NextReset(LastUpdate) < now
            ? AllowanceCap
            : Allowances;

    public static DateTime NextReset(DateTime time)
    {
        var reset = new DateTime(time.Year, time.Month, time.Day, 15, 0, 0, DateTimeKind.Utc);
        return reset < time ? reset.AddDays(1) : reset;
    }
}
