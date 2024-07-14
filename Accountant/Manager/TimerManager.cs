using System;
using System.Linq;
using Accountant.Gui.Timer;
using Accountant.SeFunctions;
using Accountant.Timers;
using Accountant.Util;

namespace Accountant.Manager;

public partial class TimerManager
{ }

public sealed partial class TimerManager : IDisposable
{
    public interface ITimerManager : IDisposable
    {
        public void                  SetState();
        public TimerWindow.BaseCache CreateCache(TimerWindow window);
    }

    public readonly RetainerTimers      RetainerTimers    = new();
    public readonly PlotCropTimers      PlotCropTimers    = new();
    public readonly PrivateCropTimers   PrivateCropTimers = new();
    public readonly SubmersibleTimers   SubmersibleTimers = new();
    public readonly AirshipTimers       AirshipTimers     = new();
    public readonly WheelTimers         WheelTimers       = new();
    public readonly TaskTimers          TaskTimers        = new();
    public readonly FreeCompanyStorage  CompanyStorage    = FreeCompanyStorage.Load();
    public readonly PositionInfoAddress PositionInfo;

    private readonly ITimerManager[] _managers;

    public TimerManager()
    {
        PositionInfo = new PositionInfoAddress(Dalamud.Log, Dalamud.SigScanner);
        _managers =
        [
            new RetainerManager(RetainerTimers),
            new CropManager(PlotCropTimers, PrivateCropTimers, PositionInfo),
            new SubmersibleManager(SubmersibleTimers, AirshipTimers, CompanyStorage),
            new AirshipManager(AirshipTimers, SubmersibleTimers, CompanyStorage),
            new WheelManager(WheelTimers, CompanyStorage),
            new TaskManager(TaskTimers),
        ];
    }

    public TimerWindow.BaseCache[] CreateCaches(TimerWindow window)
        => _managers.Select(m => m.CreateCache(window)).ToArray();

    public void CheckSettings()
    {
        foreach (var manager in _managers)
            manager.SetState();
    }

    public void Dispose()
    {
        foreach (var manager in _managers)
            manager.Dispose();
    }
}
