using Accountant.Classes;
using Accountant.Enums;
using Accountant.SeFunctions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Accountant;

public unsafe class Interop
{
    public static InternalHousingZone HousingZone
        => (InternalHousingZone)HousingManager.Instance()->GetCurrentHouseId().TerritoryTypeId;

    public static ushort Ward
        => (ushort)(HousingManager.Instance()->GetCurrentWard() + 1);

    public static bool Subdivision
        => HousingManager.Instance()->GetCurrentDivision() > 1;

    public static ushort Plot
        => (ushort)(HousingManager.Instance()->GetCurrentPlot() + 1);

    public static ushort House
        => (ushort)(HousingManager.Instance()->GetCurrentHouseId().PlotIndex + 1);

    public static PlotInfo CurrentPlot
    {
        get
        {
            var housing = HousingManager.Instance();
            var house   = housing->GetCurrentHouseId();
            var zone    = (InternalHousingZone)house.TerritoryTypeId;
            var ward    = (ushort)(housing->GetCurrentWard() + 1);
            var plot    = (ushort)(housing->IsInside() ? house.PlotIndex + 1 : housing->GetCurrentPlot() + 1);
            var world   = Dalamud.PlayerState.IsLoaded ? (ushort)Dalamud.PlayerState.CurrentWorld.RowId : (ushort)0;
            return new PlotInfo(zone, ward, plot, world);
        }
    }

    public static StaticSquadronContainer SquadronContainer    { get; private set; } = null!;
    public static UpdateGoldSaucerData    UpdateGoldSaucerData { get; private set; } = null!;

    public static void Init(IPluginLog log, ISigScanner sigScanner)
    {
        SquadronContainer    = new StaticSquadronContainer(log, sigScanner);
        UpdateGoldSaucerData = new UpdateGoldSaucerData(log, sigScanner);
    }
}
