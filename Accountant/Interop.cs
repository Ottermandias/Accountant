using Accountant.SeFunctions;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace Accountant;

public class Interop
{
    public static PositionInfoAddress     PositionInfo         { get; private set; } = null!;
    public static StaticSquadronContainer SquadronContainer    { get; private set; } = null!;
    public static UpdateGoldSaucerData    UpdateGoldSaucerData { get; private set; } = null!;

    public static void Init(IPluginLog log, ISigScanner sigScanner)
    {
        PositionInfo         = new PositionInfoAddress(log, sigScanner);
        SquadronContainer    = new StaticSquadronContainer(log, sigScanner);
        UpdateGoldSaucerData = new UpdateGoldSaucerData(log, sigScanner);
    }
}
