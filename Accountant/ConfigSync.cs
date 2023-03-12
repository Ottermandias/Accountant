using System;
using System.IO;
using Accountant.Gui.Timer;
using Accountant.Manager;
using Accountant.Timers;
using Accountant.Util;
using Dalamud.Game;
using Dalamud.Logging;

namespace Accountant;


public class ConfigSync : IDisposable
{
    private readonly TimerManager _manager;
    private readonly TimerWindow  _window;
    private          int          _frameCounter = 0;
    public ConfigSync(TimerManager manager, TimerWindow window)
    {
        _manager = manager;
        _window  = window;
        Dalamud.Framework.Update += OnFramework;
    }

    private void OnFramework(Framework _)
    {
        switch (_frameCounter++ % 9)
        {
            case 0:
                if (File.GetLastWriteTimeUtc(Dalamud.PluginInterface.ConfigFile.FullName) > Accountant.Config.LastChangeTime)
                {
                    Accountant.Config = AccountantConfiguration.Load();
                    _manager.CheckSettings();
                    _window.ResetCache();
                    PluginLog.Verbose("Reloaded Config due to external changes.");
                }

                break;
            case 1:
                CheckTimersFolder(_manager.AirshipTimers);
                break;
            case 2:
                CheckTimersFolder(_manager.PlotCropTimers);
                break;
            case 3:
                CheckTimersFolder(_manager.PrivateCropTimers);
                break;
            case 4:
                CheckTimersFolder(_manager.RetainerTimers);
                break;
            case 5:
                CheckTimersFolder(_manager.SubmersibleTimers);
                break;
            case 6:
                CheckTimersFolder(_manager.TaskTimers);
                break;
            case 7:
                CheckTimersFolder(_manager.WheelTimers);
                break;
            case 8:
                if (FreeCompanyStorage.GetWriteTime() > _manager.CompanyStorage.LastChangeTime)
                {
                    _manager.CompanyStorage.Reload();
                    _window.ResetCache();
                }
                break;
        }
    }

    private static void CheckTimersFolder<T1, T2>(TimersBase<T1, T2> timer) where T1 : struct, ITimerIdentifier
    {
        var dir     = timer.CreateFolder();
        var doStuff = dir.LastWriteTimeUtc > timer.FileChangeTime;
        if (!doStuff)
        {
            foreach (var file in dir.EnumerateFiles("*.json"))
            {
                if (file.LastWriteTimeUtc > timer.FileChangeTime)
                {
                    doStuff = true;
                    break;
                }
            }
        }

        if (!doStuff)
            return;
        timer.Reload();
        PluginLog.Verbose("Reloaded {Timer:l} due to external changes.", typeof(T2).Name);
    }

    public void Dispose()
    {
        Dalamud.Framework.Update -= OnFramework;
    }
}
