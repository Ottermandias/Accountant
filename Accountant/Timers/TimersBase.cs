using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace Accountant.Timers;

public delegate void TimerChange();

public class TimersBase<TIdent, TInfo> : ITimers<TIdent, TInfo>
    where TIdent : struct, ITimerIdentifier
{
    protected readonly Dictionary<TIdent, TInfo> InternalData = new();

    public IReadOnlyDictionary<TIdent, TInfo> Data
        => InternalData;

    public event TimerChange? Changed;
    public DateTime           FileChangeTime { get; private set; } = DateTime.UtcNow.AddMilliseconds(500);

    internal void Invoke()
    {
        PluginLog.Verbose("Change triggered in {TInfo:l}.", typeof(TInfo).Name);
        Changed?.Invoke();
    }

    public DirectoryInfo CreateFolder()
        => CreateFolder(FolderName);

    private static DirectoryInfo CreateFolder(string folderName)
    {
        var path = Path.Combine(Dalamud.PluginInterface.ConfigDirectory.FullName, folderName);
        return Directory.CreateDirectory(path);
    }

    protected virtual string FolderName
        => throw new NotImplementedException();

    protected virtual string SaveError
        => throw new NotImplementedException();

    protected virtual string ParseError
        => throw new NotImplementedException();

    protected virtual string LoadError
        => throw new NotImplementedException();

    public void Save(TIdent ident, TInfo info)
    {
        try
        {
            var dir      = CreateFolder();
            var fileName = Path.Combine(dir.FullName, $"{ident.IdentifierHash():X8}.json");
            var data     = JsonConvert.SerializeObject((ident, info), Formatting.Indented);
            File.WriteAllText(fileName, data);
            FileChangeTime = DateTime.UtcNow.AddMilliseconds(500);
        }
        catch (Exception e)
        {
            PluginLog.Error($"{SaveError}:\n{e}");
        }
    }

    public void Save(TIdent ident)
    {
        if (InternalData.TryGetValue(ident, out var info))
            Save(ident, info);
    }

    public void DeleteFile(TIdent ident)
    {
        try
        {
            var dir      = CreateFolder(FolderName);
            var fileName = Path.Combine(dir.FullName, $"{ident.IdentifierHash():X8}.json");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                FileChangeTime = DateTime.UtcNow.AddMilliseconds(500);
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Could not delete file:\n{e}");
        }
    }

    public bool Remove(TIdent ident)
    {
        if (!InternalData.Remove(ident))
            return false;

        DeleteFile(ident);
        return true;
    }

    public void Set(TIdent ident, TInfo info)
        => InternalData[ident] = info;

    public void Reload(bool condition = true)
    {
        if (!condition)
        {
            return;
        }

        InternalData.Clear();
        try
        {
            var folder = CreateFolder(FolderName);
            foreach (var file in folder.EnumerateFiles("*.json"))
            {
                try
                {
                    var data = File.ReadAllText(file.FullName);
                    var (ident, info)   = JsonConvert.DeserializeObject<(TIdent, TInfo)>(data);
                    if (ident.Valid())
                        InternalData[ident] = info;
                    else
                    {
                        PluginLog.Error($"{ParseError}:\nIdentifier was not valid.");
                        file.Delete();
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Error($"{ParseError}:\n{e}");
                }
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"{LoadError}:\n{e}");
        }
        Invoke();
        FileChangeTime = DateTime.UtcNow.AddMilliseconds(500);
    }
}
