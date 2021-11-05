using System.Linq;
using System.Reflection;
using Accountant.Gui.Config;
using Accountant.Manager;
using AddonWatcher;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using TimerWindow = Accountant.Gui.Timer.TimerWindow;

namespace Accountant;

public class Accountant : IDalamudPlugin
{
    public string Name
        => "Accountant";

    public static string Version = "";

    public static   AccountantConfiguration Config    = null!;
    public static   IGameData               GameData  = null!;
    public static   IAddonWatcher           Watcher   = null!;
    public static   RetainerManager         Retainers = null!;
    public readonly TimerManager            Timers;
    public readonly TimerWindow             TimerWindow;
    public readonly ConfigWindow            ConfigWindow;

    public Accountant(DalamudPluginInterface pluginInterface)
    {
        Dalamud.Initialize(pluginInterface);
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        Config  = AccountantConfiguration.Load();

        Watcher   = AddonWatcherFactory.Create(Dalamud.GameGui, Dalamud.SigScanner);
        GameData  = GameDataFactory.Create(Dalamud.GameGui, Dalamud.ClientState, Dalamud.GameData);
        Retainers = new RetainerManager(Dalamud.SigScanner);

        Timers       = new TimerManager();
        TimerWindow  = new TimerWindow(Timers);
        ConfigWindow = new ConfigWindow(Timers, TimerWindow);

        Dalamud.Commands.AddHandler("/accountant", new CommandInfo(OnAccountant)
        {
            HelpMessage = "Open Accountant config. Use '/accountant timers' to toggle the timer window.",
            ShowInHelp  = true,
        });
    }

    private void OnAccountant(string command, string arguments)
    {
        if (arguments.ToLowerInvariant() != "timers")
        {
            ConfigWindow.Toggle();
            return;
        }

        Config.WindowVisible = !Config.WindowVisible;
        Config.Save();
    }

    public void Dispose()
    {
        ConfigWindow.Dispose();
        TimerWindow.Dispose();
        Dalamud.Commands.RemoveHandler("/accountant");
        Timers.Dispose();
        GameData.Dispose();
        Watcher.Dispose();
    }
}
