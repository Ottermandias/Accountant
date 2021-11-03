using System;
using Accountant.Timers;

namespace Accountant.Manager;

public sealed partial class TimerManager : IDisposable
{
    private readonly IGameData       _gameData;
    public           RetainerTimers? RetainerTimers;
    public           MachineTimers?  MachineTimers;
    public           CropTimers?     CropTimers;

    public TimerManager()
    {
        _gameData = Accountant.GameData;
        _watcher  = Accountant.Watcher;
        SetupOpCodes();

        EnableRetainers(Accountant.Config.Enabled && Accountant.Config.EnableRetainers, true);
        EnableMachines(Accountant.Config.Enabled && Accountant.Config.EnableMachines, true);
        EnableCrops(Accountant.Config.Enabled && Accountant.Config.EnableCrops, true);
    }

    public void Dispose()
    {
        EnableCrops(false, true);
        EnableCrops(false, true);
        EnableCrops(false, true);
    }
}
