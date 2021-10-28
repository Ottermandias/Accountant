using System;
using System.Reflection;
using AddonWatcher;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace Accountant;

public class TimerManager : IDisposable
{
    public void Dispose()
    { }
}

public class Accountant : IDalamudPlugin
{
    public string Name
        => "Accountant";

    public static string Version = "";

    public static AccountantConfiguration Config    = null!;
    public static IGameData               GameData  = null!;
    public static IAddonWatcher           Watcher   = null!;
    public static RetainerManager         Retainers = null!;

    public Accountant(DalamudPluginInterface pluginInterface)
    {
        Dalamud.Initialize(pluginInterface);
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        Config  = AccountantConfiguration.Load();

        Watcher   = AddonWatcherFactory.Create(Dalamud.SigScanner);
        GameData  = GameDataFactory.Create(Dalamud.GameData);
        Retainers = new RetainerManager(Dalamud.SigScanner);

        Dalamud.Commands.AddHandler("/accountant", new CommandInfo(OnAccountant)
        {
            HelpMessage = "Open Accountant config.",
            ShowInHelp  = true,
        });
    }

    private void OnAccountant(string command, string arguments)
    {
        Dalamud.Chat.Print($"{Retainers.Count} Retainers");
        for (int i = 0; i < Retainers.Count; ++i)
        {
            var retainer = Retainers.Retainer(i);
            Dalamud.Chat.Print($"{retainer.Name} - {retainer.Gil} {retainer.ClassJob} {retainer.RetainerID} {retainer.VentureComplete.ToLocalTime()}");
        }
    }

    public void Dispose()
    {
        Dalamud.Commands.RemoveHandler("/accountant");
        Watcher.Dispose();
    }
}
