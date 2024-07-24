using System;
using System.Collections.Generic;
using System.Linq;
using Accountant.Gui;
using Dalamud.Configuration;
using Newtonsoft.Json;

namespace Accountant;

[Flags]
public enum ConfigFlags : uint
{
    None               = 0x000000,
    Enabled            = 0x000001,
    WindowVisible      = 0x000002,
    Retainers          = 0x000004,
    RetainersDtrTimer  = 0x000008,
    Airships           = 0x000010,
    Submersibles       = 0x000020,
    AetherialWheels    = 0x000040,
    Crops              = 0x000080,
    OrderByCrop        = 0x000100,
    LeveAllowances     = 0x000200,
    HideDisabled       = 0x000400,
    Squadron           = 0x000800,
    MapAllowance       = 0x001000,
    ProhibitResize     = 0x002000,
    ProhibitMoving     = 0x004000,
    IgnoreIndoorPlants = 0x008000,
    MiniCactpot        = 0x010000,
    JumboCactpot       = 0x020000,
    NoHeaderStyling    = 0x040000,
    CustomDelivery     = 0x080000,
    Tribes             = 0x100000,
}

public static class ConfigFlagExtensions
{
    public static bool Any(this ConfigFlags flags, ConfigFlags flag)
        => (flags & flag) != 0;

    public static bool Check(this ConfigFlags flags, ConfigFlags flag)
        => (flags & flag) == flag;

    public static bool Set(this ref ConfigFlags flags, ConfigFlags flag, bool on)
    {
        if (on)
        {
            if (flags.Check(flag))
                return false;

            flags |= flag;
            return true;
        }

        if (!flags.Any(flag))
            return false;

        flags &= ~flag;
        return true;
    }
}

public class AccountantConfiguration : IPluginConfiguration
{
    public const ConfigFlags DefaultFlags =
        ConfigFlags.Enabled
      | ConfigFlags.WindowVisible
      | ConfigFlags.Retainers
      | ConfigFlags.RetainersDtrTimer
      | ConfigFlags.Airships
      | ConfigFlags.Submersibles
      | ConfigFlags.Crops
      | ConfigFlags.AetherialWheels
      | ConfigFlags.Squadron
      | ConfigFlags.MapAllowance
      | ConfigFlags.MiniCactpot
      | ConfigFlags.JumboCactpot
      | ConfigFlags.LeveAllowances
      | ConfigFlags.CustomDelivery
      | ConfigFlags.Tribes;

    public int Version { get; set; } = 4;

    [JsonIgnore]
    public ConfigFlags Flags = DefaultFlags;

    public bool Enabled
    {
        get => Flags.Check(ConfigFlags.Enabled);
        set => Flags.Set(ConfigFlags.Enabled, value);
    }

    public bool WindowVisible
    {
        get => Flags.Check(ConfigFlags.WindowVisible);
        set => Flags.Set(ConfigFlags.WindowVisible, value);
    }

    public bool EnableRetainers
    {
        get => Flags.Check(ConfigFlags.Retainers);
        set => Flags.Set(ConfigFlags.Retainers, value);
    }

    public bool EnableRetainersDtrTimer
    {
        get => Flags.Check(ConfigFlags.RetainersDtrTimer);
        set => Flags.Set(ConfigFlags.RetainersDtrTimer, value);
    }

    public bool EnableAirships
    {
        get => Flags.Check(ConfigFlags.Airships);
        set => Flags.Set(ConfigFlags.Airships, value);
    }

    public bool EnableSubmersibles
    {
        get => Flags.Check(ConfigFlags.Submersibles);
        set => Flags.Set(ConfigFlags.Submersibles, value);
    }

    public bool EnableWheels
    {
        get => Flags.Check(ConfigFlags.AetherialWheels);
        set => Flags.Set(ConfigFlags.AetherialWheels, value);
    }

    public bool EnableCrops
    {
        get => Flags.Check(ConfigFlags.Crops);
        set => Flags.Set(ConfigFlags.Crops, value);
    }

    public bool OrderByCrop
    {
        get => Flags.Check(ConfigFlags.OrderByCrop);
        set => Flags.Set(ConfigFlags.OrderByCrop, value);
    }

    public bool EnableLeveAllowances
    {
        get => Flags.Check(ConfigFlags.LeveAllowances);
        set => Flags.Set(ConfigFlags.LeveAllowances, value);
    }

    public bool EnableSquadron
    {
        get => Flags.Check(ConfigFlags.Squadron);
        set => Flags.Set(ConfigFlags.Squadron, value);
    }

    public bool HideDisabled
    {
        get => Flags.Check(ConfigFlags.HideDisabled);
        set => Flags.Set(ConfigFlags.HideDisabled, value);
    }

    public bool EnableMapAllowance
    {
        get => Flags.Check(ConfigFlags.MapAllowance);
        set => Flags.Set(ConfigFlags.MapAllowance, value);
    }

    public bool ProhibitResize
    {
        get => Flags.Check(ConfigFlags.ProhibitResize);
        set => Flags.Set(ConfigFlags.ProhibitResize, value);
    }

    public bool ProhibitMoving
    {
        get => Flags.Check(ConfigFlags.ProhibitMoving);
        set => Flags.Set(ConfigFlags.ProhibitMoving, value);
    }

    public bool IgnoreIndoorPlants
    {
        get => Flags.Check(ConfigFlags.IgnoreIndoorPlants);
        set => Flags.Set(ConfigFlags.IgnoreIndoorPlants, value);
    }

    public bool EnableMiniCactpot
    {
        get => Flags.Check(ConfigFlags.MiniCactpot);
        set => Flags.Set(ConfigFlags.MiniCactpot, value);
    }

    public bool EnableJumboCactpot
    {
        get => Flags.Check(ConfigFlags.JumboCactpot);
        set => Flags.Set(ConfigFlags.JumboCactpot, value);
    }

    public bool NoHeaderStyling
    {
        get => Flags.Check(ConfigFlags.NoHeaderStyling);
        set => Flags.Set(ConfigFlags.NoHeaderStyling, value);
    }

    public bool EnableDeliveries
    {
        get => Flags.Check(ConfigFlags.CustomDelivery);
        set => Flags.Set(ConfigFlags.CustomDelivery, value);
    }

    public bool EnableTribes
    {
        get => Flags.Check(ConfigFlags.Tribes);
        set => Flags.Set(ConfigFlags.Tribes, value);
    }

    public int LeveWarning    { get; set; } = 85;
    public int TribesFinished { get; set; } = 0;

    public bool ShowCropTooltip = true;
    public bool ShowDtr { get; set; } = true;

    public Dictionary<ColorId, uint> Colors { get; set; } = Enum.GetValues<ColorId>().ToDictionary(c => c, c => c.Default());

    public SortedList<ulong, string> PlotNames { get; } = new();

    public HashSet<ulong>  BlockedPlots                 { get; } = [];
    public HashSet<string> BlockedPlayersCrops          { get; } = [];
    public HashSet<string> BlockedPlayersRetainers      { get; } = [];
    public HashSet<string> BlockedPlayersTasks          { get; } = [];
    public HashSet<string> BlockedCompaniesAirships     { get; } = [];
    public HashSet<string> BlockedCompaniesSubmersibles { get; } = [];
    public HashSet<string> BlockedCompaniesWheels       { get; } = [];
    public HashSet<uint>   BlockedCrops                 { get; } = [];

    public Dictionary<string, int> Priorities { get; } = new();

    public int GetPriority(string name)
        => Priorities.GetValueOrDefault(name, int.MinValue);

    [JsonIgnore]
    public DateTime LastChangeTime { get; private set; } = DateTime.UtcNow.AddMilliseconds(500);

    public float? FixedWindowWidth { get; set; } = null;

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
    {
        Dalamud.PluginInterface.SavePluginConfig(this);
        LastChangeTime = DateTime.UtcNow.AddMilliseconds(500);
    }


    // Backwards-Compatibility
    public bool EnableMachines
    {
        set => Flags.Set(ConfigFlags.Airships | ConfigFlags.Submersibles, value);
    }
}
