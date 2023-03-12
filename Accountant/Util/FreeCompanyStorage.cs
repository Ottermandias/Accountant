using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accountant.Classes;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace Accountant.Util;

public class FreeCompanyStorage
{
    public const string FileName = "free_company_data.json";

    public readonly List<FreeCompanyInfo> Infos = new();

    [JsonIgnore]
    public DateTime LastChangeTime { get; set; } = DateTime.UtcNow.AddMilliseconds(500);

    public FreeCompanyInfo? GetCurrentCompanyInfo()
    {
        if (!Accountant.GameData.Valid)
            return null;

        if (Dalamud.ClientState.LocalPlayer == null)
            return null;

        var (tag, name, leader) = Accountant.GameData.FreeCompanyInfo();
        var id = (ushort)Dalamud.ClientState.LocalPlayer.HomeWorld.Id;
        return FindByAndUpdateInfo(tag, name, leader, id);
    }

    private static FileInfo FileInfo
        => new(Path.Combine(Dalamud.PluginInterface.GetPluginConfigDirectory(), FileName));

    public static DateTime GetWriteTime()
        => FileInfo.LastWriteTimeUtc;

    public FreeCompanyInfo? FindByLeader(string leader, uint serverId)
        => Infos.FirstOrDefault(f => f.Leader == leader && f.ServerId == serverId);

    public FreeCompanyInfo? FindByTag(string tag, uint serverId)
        => Infos.FirstOrDefault(f => f.Tag == tag && f.ServerId == serverId);

    public FreeCompanyInfo? FindByName(string name, uint serverId)
        => Infos.FirstOrDefault(f => f.Name == name && f.ServerId == serverId);

    public FreeCompanyInfo? FindByAndUpdateInfo(string tag, string? name, string? leader, ushort serverId)
    {
        if (tag.Length == 0)
            return null;

        var l = leader ?? string.Empty;
        var n = name ?? string.Empty;


        if (n.Length == 0)
        {
            var infos = Infos.Where(i => i.ServerId == serverId);
            return l.Length > 0
                ? infos.Cast<FreeCompanyInfo?>().FirstOrDefault(i => i!.Value.Tag == tag && i.Value.Leader == l)
                : infos.Cast<FreeCompanyInfo?>().FirstOrDefault(i => i!.Value.Tag == tag);
        }

        var idx = Infos.FindIndex(i => i.Name == n && i.ServerId == serverId);
        if (idx == -1 && serverId != 0)
        {
            if (l.Length == 0)
                return null;

            Infos.Add(new FreeCompanyInfo(n, serverId)
            {
                Leader = l,
                Tag    = tag,
            });
            Save();
            return Infos.Last();
        }

        if (Infos[idx].Tag == tag && Infos[idx].Leader == l)
            return Infos[idx];

        Infos[idx] = new FreeCompanyInfo(n, serverId)
        {
            Leader = l,
            Tag    = tag,
        };
        Save();
        return Infos[idx];
    }

    public void Reload()
    {
        var info = Load();
        Infos.Clear();
        Infos.AddRange(info.Infos);
    }

    public static FreeCompanyStorage Load()
    {
        var file = FileInfo;
        if (file.Exists)
            try
            {
                var data    = File.ReadAllText(file.FullName);
                var storage = JsonConvert.DeserializeObject<FreeCompanyStorage>(data)!;
                if (storage.Infos.RemoveAll(f => f.ServerId == 0) > 0)
                    storage.Save();
                return storage;
            }
            catch (Exception e)
            {
                PluginLog.Error($"Could not read free company storage data:\n{e}");
            }

        var newStorage = new FreeCompanyStorage();
        newStorage.Save();
        return newStorage;
    }

    public void Save()
    {
        try
        {
            var file = FileInfo;
            var data = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(file.FullName, data);
            LastChangeTime = DateTime.UtcNow.AddMilliseconds(500);
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not save free company storage data:\n{e}");
        }
    }
}
