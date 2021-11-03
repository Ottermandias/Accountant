using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accountant.Classes;
using Accountant.Util;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace Accountant.Timers;

public class RetainerTimers
{
    public const     string                                 FolderName = "retainers";
    private readonly Dictionary<PlayerInfo, RetainerInfo[]> _retainers = new();

    public event TimerChange? RetainerChanged;

    public IReadOnlyDictionary<PlayerInfo, RetainerInfo[]> Retainers
        => _retainers;

    public bool AddOrUpdateRetainer(PlayerInfo player, RetainerInfo retainer, byte slot)
    {
        if (slot >= RetainerInfo.MaxSlots)
        {
            PluginLog.Error($"Only {RetainerInfo.MaxSlots} Retainers supported.");
            return false;
        }

        if (retainer.RetainerId == 0)
            return false;

        if (!_retainers.TryGetValue(player, out var retainerList))
        {
            retainerList       = Enumerable.Repeat(RetainerInfo.None, RetainerInfo.MaxSlots).ToArray();
            retainerList[slot] = retainer;
            _retainers[player] = retainerList;
            RetainerChanged?.Invoke();
            return true;
        }

        var oldRetainer = retainerList[slot];
        if (Helpers.DateTimeClose(oldRetainer.Venture, retainer.Venture)
         && oldRetainer.VentureId == retainer.VentureId
         && oldRetainer.RetainerId == retainer.RetainerId
         && oldRetainer.Name == retainer.Name
         && oldRetainer.JobId == retainer.JobId)
            return false;

        retainerList[slot] = retainer;
        RetainerChanged?.Invoke();
        return true;
    }

    public bool RemovePlayer(PlayerInfo player)
    {
        if (!_retainers.Remove(player))
            return false;

        RetainerChanged?.Invoke();
        return true;
    }

    public bool RemoveRetainer(PlayerInfo player, byte slot)
    {
        if (!_retainers.TryGetValue(player, out var retainerList))
            return false;

        if (slot > RetainerInfo.MaxSlots)
            return false;

        var ret = retainerList[slot].RetainerId != 0;
        retainerList[slot] = RetainerInfo.None;

        if (!ret)
            return false;

        RetainerChanged?.Invoke();
        return true;
    }

    public bool RemoveRetainer(PlayerInfo player, RetainerInfo retainer)
    {
        if (retainer.RetainerId == 0)
            return false;

        if (!_retainers.TryGetValue(player, out var retainerList))
            return false;

        var slot = Array.IndexOf(retainerList, retainer);
        if (slot < 0)
            return false;

        retainerList[slot] = RetainerInfo.None;
        RetainerChanged?.Invoke();
        return true;
    }

    private static DirectoryInfo CreateFolder()
    {
        var path = Path.Combine(Dalamud.PluginInterface.ConfigDirectory.FullName, FolderName);
        return Directory.CreateDirectory(path);
    }

    public void Save(PlayerInfo player, RetainerInfo[] retainers)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{player.GetStableHashCode():X8}.json");
            var data     = JsonConvert.SerializeObject((player, retainers), Formatting.Indented);
            File.WriteAllText(fileName, data);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not write retainer data:\n{e}");
        }
    }

    public void Save(PlayerInfo player)
    {
        if (_retainers.TryGetValue(player, out var retainers))
            Save(player, retainers);
    }

    public void DeleteFile(PlayerInfo player)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{player.GetStableHashCode():X8}.json");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not delete file:\n{e}");
        }
    }

    public static RetainerTimers Load()
    {
        var timers = new RetainerTimers();
        try
        {
            var folder = CreateFolder();
            foreach (var file in folder.EnumerateFiles("*.json"))
            {
                try
                {
                    var data = File.ReadAllText(file.FullName);
                    var (player, retainers)   = JsonConvert.DeserializeObject<(PlayerInfo, RetainerInfo[])>(data);
                    timers._retainers[player] = retainers;
                }
                catch (Exception e)
                {
                    PluginLog.Error($"Invalid retainer data file {file} could not be parsed:\n{e}");
                }
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Error loading retainer timers:\n{e}");
        }

        return timers;
    }
}
