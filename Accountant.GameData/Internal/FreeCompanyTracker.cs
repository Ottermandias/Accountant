using System;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin.Services;

namespace Accountant.Internal;

internal class FreeCompanyTracker
{
    private readonly IClientState _state;
    private readonly IFramework   _framework;
    private readonly IntPtr       _fcStatePtr  = IntPtr.Zero;
    private readonly IntPtr       _fcNamePtr   = IntPtr.Zero;
    private readonly IntPtr       _fcLeaderPtr = IntPtr.Zero;
    private          string?      _freeCompanyName;
    private          string?      _freeCompanyLeader;
    private          string       _characterName  = string.Empty;
    private          string       _freeCompanyTag = string.Empty;
    private          uint         _serverId;

    private unsafe SeString? GetFcInfo(IntPtr ptr)
    {
        if (_fcStatePtr == IntPtr.Zero)
            return null;

        if (*(byte*)ptr != 0)
            return MemoryHelper.ReadSeStringNullTerminated(ptr);
        if (*(ulong*)_fcStatePtr != 0)
            return null;

        return SeString.Empty;
    }

    private SeString? GetFcName()
        => GetFcInfo(_fcNamePtr);

    private SeString? GetFcLeader()
        => GetFcInfo(_fcLeaderPtr);

    private void Update()
    {
        if (_state.IsLoggedIn && _state.LocalPlayer != null)
        {
            var newName   = _state.LocalPlayer.Name.TextValue;
            var newTag    = _state.LocalPlayer.CompanyTag.TextValue;
            var newServer = _state.LocalPlayer.HomeWorld.RowId;
            if (_characterName != newName || _freeCompanyTag != newTag || newServer != _serverId)
            {
                _freeCompanyName   = null;
                _freeCompanyLeader = null;
            }

            _characterName  = newName;
            _freeCompanyTag = newTag;
            _serverId       = newServer;

            var newCompany = GetFcName()?.TextValue;
            var newLeader  = GetFcLeader()?.TextValue;
            if (newCompany != null && (_freeCompanyName == null || newCompany.Length > 0))
                _freeCompanyName = newCompany;

            if (newLeader != null && (_freeCompanyLeader == null || newLeader.Length > 0))
                _freeCompanyLeader = newLeader;
        }
        else
        {
            _characterName     = string.Empty;
            _freeCompanyTag    = string.Empty;
            _serverId          = 0;
            _freeCompanyLeader = null;
            _freeCompanyName   = null;
        }
    }

    private static unsafe IntPtr GetDataPointer(IPluginLog log, IGameGui gui)
    {
        var uiModule = gui.GetUIModule();
        var vf34     = (IntPtr*)(*(ulong*)uiModule + 8 * Offsets.FreeCompany.FreeCompanyModuleVfunc);
        log.Verbose($"Obtained free company data module getter (VFunc 34 of uiModule) at 0x{(ulong)vf34:X16}.");
        var module = ((delegate*<IntPtr, IntPtr>)*vf34)(uiModule);
        log.Verbose($"Obtained free company data module at 0x{module:X16}.");
        var data = *(IntPtr*)(module + Offsets.FreeCompany.DataOffset);
        if (data == IntPtr.Zero)
            log.Error("Could not obtain free company data.");
        return data;
    }

    public FreeCompanyTracker(IPluginLog log, IGameGui gui, IClientState state, IFramework framework)
    {
        _state     = state;
        _framework = framework;
        var fcData = GetDataPointer(log, gui);
        if (fcData != IntPtr.Zero)
        {
            _fcStatePtr  = fcData + 0x48;
            _fcNamePtr   = fcData + 0x7C;
            _fcLeaderPtr = fcData + 0x93;
        }

        _state.Login  += LoginHandler;
        _state.Logout += LogoutHandler;
    }

    private void LoginHandler()
        => _framework.Update += UpdateAndRemove;

    private void LogoutHandler(int _1, int _2)
    {
        Update();
    }

    private void UpdateAndRemove(IFramework _)
    {
        if (_state.LocalPlayer == null)
            return;

        Update();
        _framework.Update -= UpdateAndRemove;
    }

    public void Dispose()
    {
        _state.Login      -= LoginHandler;
        _state.Logout     -= LogoutHandler;
        _framework.Update -= UpdateAndRemove;
    }

    public (string Tag, string? Name, string? Leader) FreeCompanyInfo
    {
        get
        {
            Update();
            return (_freeCompanyTag, _freeCompanyName, _freeCompanyLeader);
        }
    }
}
