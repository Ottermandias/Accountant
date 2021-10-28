using Dalamud.Configuration;

namespace Accountant;

public class AccountantConfiguration : IPluginConfiguration
{
    public int  Version { get; set; } = 2;
    public bool Enabled { get; set; } = true;

    public static AccountantConfiguration Load()
    {
        if (Dalamud.PluginInterface.GetPluginConfig() is AccountantConfiguration cfg)
            return cfg;

        cfg = new AccountantConfiguration();
        cfg.Save();

        return cfg;
    }

    public void Save()
        => Dalamud.PluginInterface.SavePluginConfig(this);
}
