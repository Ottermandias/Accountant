using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accountant.Classes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace Accountant.Util;

public class FreeCompanyStorage
{
    public const string FileName = "free_company_data.json";

    public List<FreeCompanyInfo> Infos = new();

    public FreeCompanyInfo? GetCurrentCompanyInfo()
    {
        if (!Accountant.GameData.Valid)
            return null;

        if (Dalamud.ClientState.LocalPlayer == null)
            return null;

        var (tag, name, leader) = Accountant.GameData.FreeCompanyInfo();
        var id = (ushort) Dalamud.ClientState.LocalPlayer.HomeWorld.Id;
        return FindByAndUpdateInfo(tag, name, leader, id);
    }

    private static FileInfo FileInfo
        => new(Path.Combine(Dalamud.PluginInterface.GetPluginConfigDirectory(), FileName));

    public FreeCompanyInfo? FindByLeader(string leader, uint serverId)
        => Infos.FirstOrDefault(f => f.Leader == leader && f.ServerId == serverId);

    public FreeCompanyInfo? FindByTag(string tag, uint serverId)
        => Infos.FirstOrDefault(f => f.Tag == tag && f.ServerId == serverId);

    public FreeCompanyInfo? FindByName(string name, uint serverId)
        => Infos.FirstOrDefault(f => f.Name == name && f.ServerId == serverId);

    public FreeCompanyInfo? FindByAndUpdateInfo(SeString tag, SeString? name, SeString? leader, ushort serverId)
    {
        if (tag == SeString.Empty)
            return null;

        var t     = tag.TextValue;
        var l     = leader?.TextValue ?? string.Empty;
        var n     = name?.TextValue ?? string.Empty;
        var infos = Infos.Where(i => i.ServerId == serverId);

        if (!n.Any())
            return l.Any()
                ? infos.FirstOrDefault(i => i.Tag == t && i.Leader == l)
                : infos.FirstOrDefault(i => i.Tag == t);

        var i = infos.FirstOrDefault(i => i.Name == n);
        if (i.ServerId == 0)
        {
            if (!l.Any())
                return null;

            Infos.Add(new FreeCompanyInfo(n, serverId)
            {
                Leader = l,
                Tag    = t,
            });
            Save();
            return Infos.Last();
        }

        if (i.Tag == t && i.Leader == l)
            return i;

        i.Tag    = t;
        i.Leader = l;
        Save();
        return i;
    }

    public static FreeCompanyStorage Load()
    {
        var file = FileInfo;
        if (file.Exists)
            try
            {
                var data    = File.ReadAllText(file.FullName);
                var storage = JsonConvert.DeserializeObject<FreeCompanyStorage>(data);
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
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not save free company storage data:\n{e}");
        }
    }
}
