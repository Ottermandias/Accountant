using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace AddonWatcher.SeFunctions;

public class SeFunctionBase<T> where T : Delegate
{
    private readonly IPluginLog _log;
    public           IntPtr     Address;
    protected        T?         FuncDelegate;

    public SeFunctionBase(IPluginLog log, ISigScanner sigScanner, int offset)
    {
        _log    = log;
        Address = sigScanner.Module.BaseAddress + offset;
        _log.Debug($"{GetType().Name} address 0x{Address.ToInt64():X16}, baseOffset 0x{offset:X16}.");
    }

    public SeFunctionBase(IPluginLog log, ISigScanner sigScanner, string signature, int offset = 0)
    {
        _log    = log;
        Address = sigScanner.ScanText(signature);
        if (Address != IntPtr.Zero)
            Address += offset;
        var baseOffset = (ulong)Address.ToInt64() - (ulong)sigScanner.Module.BaseAddress.ToInt64();
        _log.Debug($"{GetType().Name} address 0x{Address.ToInt64():X16}, baseOffset 0x{baseOffset:X16}.");
    }

    public T? Delegate()
    {
        if (FuncDelegate != null)
            return FuncDelegate;

        if (Address != IntPtr.Zero)
        {
            FuncDelegate = Marshal.GetDelegateForFunctionPointer<T>(Address);
            return FuncDelegate;
        }

        _log.Error($"Trying to generate delegate for {GetType().Name}, but no pointer available.");
        return null;
    }

    public dynamic? Invoke(params dynamic[] parameters)
    {
        if (FuncDelegate != null)
            return FuncDelegate.DynamicInvoke(parameters);

        if (Address != IntPtr.Zero)
        {
            FuncDelegate = Marshal.GetDelegateForFunctionPointer<T>(Address);
            return FuncDelegate!.DynamicInvoke(parameters);
        }

        _log.Error($"Trying to call {GetType().Name}, but no pointer available.");
        return null;
    }

    public Hook<T>? CreateHook(IGameInteropProvider provider, T detour, bool enable = true)
    {
        if (Address != IntPtr.Zero)
        {
            var hook = provider.HookFromAddress(Address, detour);
            if (enable)
                hook.Enable();
            _log.Debug($"Hooked onto {GetType().Name} at address 0x{Address.ToInt64():X16}.");
            return hook;
        }

        _log.Error($"Trying to create Hook for {GetType().Name}, but no pointer available.");
        return null;
    }
}
