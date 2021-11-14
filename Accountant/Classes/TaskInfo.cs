using System;

namespace Accountant.Classes;

public class TaskInfo
{
    public Leve     Leves    = new();
    public Squadron Squadron = new();
    public DateTime Map      = DateTime.MinValue;
}
