using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Gui;
using Dalamud.Configuration;

namespace Accountant;

public class AccountantConfiguration : IPluginConfiguration
{
    public int  Version         { get; set; } = 2;
    public bool Enabled         { get; set; } = true;
    public bool WindowVisible   { get; set; } = true;
    public bool EnableRetainers { get; set; } = true;
    public bool EnableMachines  { get; set; } = true;
    public bool EnableCrops     { get; set; } = true;
    public bool OrderByCrop     { get; set; } = false;

    public Dictionary<ColorId, uint> Colors { get; set; } = Enum.GetValues<ColorId>().ToDictionary(c => c, c => c.Default());

    public SortedList<ulong, string> PlotNames { get; } = new();

    public HashSet<ulong>  BlockedPlots     { get; } = new();
    public HashSet<string> BlockedPlayers   { get; } = new();
    public HashSet<string> BlockedCompanies { get; } = new();

    public static AccountantConfiguration Load()
    {
        var save = false;
        if (Dalamud.PluginInterface.GetPluginConfig() is not AccountantConfiguration cfg)
        {
            cfg  = new AccountantConfiguration();
            save = true;
        }
        else
        {
            foreach (var color in Enum.GetValues<ColorId>())
                save |= cfg.Colors.TryAdd(color, color.Default());
        }

        if (save)
            cfg.Save();

        return cfg;
    }

    public void Save()
        => Dalamud.PluginInterface.SavePluginConfig(this);
}
