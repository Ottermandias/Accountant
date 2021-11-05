using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Memory;

namespace Accountant.Internal;

internal class FreeCompanyTracker
{
    private readonly ClientState _state;
    private readonly IntPtr      _fcStatePtr  = IntPtr.Zero;
    private readonly IntPtr      _fcNamePtr   = IntPtr.Zero;
    private readonly IntPtr      _fcLeaderPtr = IntPtr.Zero;
    private          SeString?   _freeCompanyName;
    private          SeString?   _freeCompanyLeader;
    private          SeString    _characterName  = SeString.Empty;
    private          SeString    _freeCompanyTag = SeString.Empty;
    private          uint        _serverId;

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
            var newName   = _state.LocalPlayer.Name;
            var newTag    = _state.LocalPlayer.CompanyTag;
            var newServer = _state.LocalPlayer.HomeWorld.Id;
            if (_characterName != newName || _freeCompanyTag != newTag || newServer != _serverId)
            {
                _freeCompanyName   = null;
                _freeCompanyLeader = null;
            }

            _characterName  = newName;
            _freeCompanyTag = newTag;
            _serverId       = newServer;

            var newCompany = GetFcName();
            var newLeader  = GetFcLeader();
            if (newCompany != null && (_freeCompanyName == null || newCompany != SeString.Empty))
                _freeCompanyName = newCompany;
            if (newLeader != null && (_freeCompanyLeader == null || newLeader != SeString.Empty))
                _freeCompanyLeader = newLeader;
        }
        else
        {
            _characterName     = SeString.Empty;
            _freeCompanyTag    = SeString.Empty;
            _serverId          = 0;
            _freeCompanyLeader = null;
            _freeCompanyName   = null;
        }
    }

    private static unsafe IntPtr GetDataPointer(GameGui gui)
    {
        var uiModule = gui.GetUIModule();
        var vf33     = (IntPtr*)(*(ulong*)uiModule + 0x108);
        PluginLog.Verbose($"Obtained free company data module getter (VFunc 33 of uiModule) at 0x{(ulong)vf33:X16}.");
        var module = ((delegate*<IntPtr, IntPtr>)*vf33)(uiModule);
        PluginLog.Verbose($"Obtained free company data module at 0x{module:X16}.");
        var data = *(IntPtr*)(module + 0x1978 + 0x8 * 0xB);
        if (data == IntPtr.Zero)
            PluginLog.Error("Could not obtain free company data.");
        return data;
    }

    public FreeCompanyTracker(GameGui gui, ClientState state)
    {
        _state = state;
        var fcData = GetDataPointer(gui);
        if (fcData != IntPtr.Zero)
        {
            _fcStatePtr  = fcData + 0x48;
            _fcNamePtr   = fcData + 0x7C;
            _fcLeaderPtr = fcData + 0x93;
        }

        _state.Login  += UpdateHandler;
        _state.Logout += UpdateHandler;
    }

    private void UpdateHandler(object? _, EventArgs _2)
        => Update();

    public void Dispose()
    {
        _state.Login  -= UpdateHandler;
        _state.Logout -= UpdateHandler;
    }

    public (SeString Tag, SeString? Name, SeString? Leader) FreeCompanyInfo
    {
        get
        {
            Update();
            return (_freeCompanyTag, _freeCompanyName, _freeCompanyLeader);
        }
    }
}
