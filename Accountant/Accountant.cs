using System.Reflection;
using System.Threading.Tasks;
using Accountant.Gui;
using Accountant.Manager;
using AddonWatcher;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;

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
        ConfigWindow = new ConfigWindow();

        Dalamud.Commands.AddHandler("/accountant", new CommandInfo(OnAccountant)
        {
            HelpMessage = "Open Accountant config.",
            ShowInHelp  = true,
        });
    }

    private void OnAccountant(string command, string arguments)
        => ConfigWindow.Toggle();

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
