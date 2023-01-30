using System;

namespace Accountant.Classes;

public class TaskInfo
{
    public Leve         Leves        = new();
    public Squadron     Squadron     = new();
    public DateTime     Map          = DateTime.MinValue;
    public MiniCactpot  MiniCactpot  = new();
    public JumboCactpot JumboCactpot = new();
    public Delivery     Delivery     = new();
    public Tribe        Tribe        = new();
}
