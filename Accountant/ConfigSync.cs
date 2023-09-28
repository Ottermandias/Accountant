using System;
using System.IO;
using System.Linq;
using Accountant.Gui.Timer;
using Accountant.Manager;
using Accountant.Timers;
using Accountant.Util;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

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

    private void OnFramework(IFramework _)
    {
        switch (_frameCounter++ % 128)
        {
            case 0:
                if (File.GetLastWriteTimeUtc(Dalamud.PluginInterface.ConfigFile.FullName) > Accountant.Config.LastChangeTime)
                {
                    Accountant.Config = AccountantConfiguration.Load();
                    _manager.CheckSettings();
                    _window.ResetCache();
                    Dalamud.Log.Verbose("Reloaded Config due to external changes.");
                }

                break;
            case 10:
                CheckTimersFolder(_manager.AirshipTimers);
                break;
            case 20:
                CheckTimersFolder(_manager.PlotCropTimers);
                break;
            case 30:
                CheckTimersFolder(_manager.PrivateCropTimers);
                break;
            case 40:
                CheckTimersFolder(_manager.RetainerTimers);
                break;
            case 50:
                CheckTimersFolder(_manager.SubmersibleTimers);
                break;
            case 60:
                CheckTimersFolder(_manager.TaskTimers);
                break;
            case 70:
                CheckTimersFolder(_manager.WheelTimers);
                break;
            case 80:
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
        if (dir.LastWriteTimeUtc <= timer.FileChangeTime
         && !dir.EnumerateFiles("*.json").Any(file => file.LastWriteTimeUtc > timer.FileChangeTime))
            return;

        timer.Reload();
        Dalamud.Log.Verbose("Reloaded {Timer:l} due to external changes.", typeof(T2).Name);
    }

    public void Dispose()
    {
        Dalamud.Framework.Update -= OnFramework;
    }
}
