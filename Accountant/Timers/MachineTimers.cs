using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accountant.Classes;
using Accountant.Enums;
using Accountant.Util;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace Accountant.Timers;

public class MachineTimers
{
    public const     string                                     FolderName = "machines";
    private readonly Dictionary<FreeCompanyInfo, MachineInfo[]> _machines  = new();

    public IReadOnlyDictionary<FreeCompanyInfo, MachineInfo[]> Machines
        => _machines;

    public event TimerChange? MachineChanged;

    public bool AddOrUpdateMachine(FreeCompanyInfo company, MachineInfo machine, byte slot)
    {
        if (slot >= MachineInfo.MaxSlots)
        {
            PluginLog.Error($"Only {MachineInfo.MaxSlots} {machine.Type}s supported.");
            return false;
        }

        if (machine.Type == MachineType.Unknown || !machine.Name.Any())
            return false;

        slot = (byte)machine.Slot(slot);
        if (!_machines.TryGetValue(company, out var machineList))
        {
            machineList        = Enumerable.Repeat(MachineInfo.None, MachineInfo.MaxSlots * MachineInfo.Types).ToArray();
            machineList[slot]  = machine;
            _machines[company] = machineList;
            MachineChanged?.Invoke();
            return true;
        }

        var oldMachine = machineList[slot];
        // Do not need to check for type by slot.
        if (Helpers.DateTimeClose(oldMachine.Arrival, machine.Arrival) && oldMachine.Name == machine.Name)
            return false;

        machineList[slot] = machine;
        MachineChanged?.Invoke();
        return true;
    }

    public bool RemoveCompany(FreeCompanyInfo company)
        => _machines.Remove(company);

    public bool RemoveMachine(FreeCompanyInfo company, MachineInfo machine)
    {
        if (machine.Type == MachineType.Unknown)
            return false;

        if (!_machines.TryGetValue(company, out var machineList))
            return false;

        var slot = Array.IndexOf(machineList, machine);
        if (slot < 0)
            return false;

        machineList[slot] = MachineInfo.None;
        MachineChanged?.Invoke();
        return true;
    }

    private static DirectoryInfo CreateFolder()
    {
        var path = Path.Combine(Dalamud.PluginInterface.ConfigDirectory.FullName, FolderName);
        return Directory.CreateDirectory(path);
    }

    public void Save(FreeCompanyInfo company, MachineInfo[] machines)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{company.GetStableHashCode():X8}.json");
            var data     = JsonConvert.SerializeObject((company, machines), Formatting.Indented);
            File.WriteAllText(fileName, data);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write machine data:\n{e}");
        }
    }

    public void Save(FreeCompanyInfo company)
    {
        if (_machines.TryGetValue(company, out var machines))
            Save(company, machines);
    }

    public void DeleteFile(FreeCompanyInfo company)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{company.GetStableHashCode():X8}.json");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not delete file:\n{e}");
        }
    }

    public static MachineTimers Load()
    {
        var timers = new MachineTimers();
        try
        {
            var folder = CreateFolder();
            foreach (var file in folder.EnumerateFiles("*.json"))
            {
                try
                {
                    var data = File.ReadAllText(file.FullName);
                    var (company, machines)   = JsonConvert.DeserializeObject<(FreeCompanyInfo, MachineInfo[])>(data);
                    timers._machines[company] = machines;
                }
                catch (Exception e)
                {
                    PluginLog.Error($"Invalid machine data file {file} could not be parsed:\n{e}");
                }
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading machine timers:\n{e}");
        }

        return timers;
    }
}
