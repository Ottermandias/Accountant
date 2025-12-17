using System.Diagnostics.CodeAnalysis;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Accountant;

public class Dalamud
{
    public static void Initialize(IDalamudPluginInterface pluginInterface)
        => pluginInterface.Create<Dalamud>();

        // @formatter:off
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager         Commands        { get; private set; } = null!;
        [PluginService] public static ISigScanner             SigScanner      { get; private set; } = null!;
        [PluginService] public static IDataManager            GameData        { get; private set; } = null!;
        [PluginService] public static IObjectTable            Objects         { get; private set; } = null!;
        [PluginService] public static IClientState            ClientState     { get; private set; } = null!;
        [PluginService] public static IPlayerState            PlayerState     { get; private set; } = null!;
        [PluginService] public static IChatGui                Chat            { get; private set; } = null!;
        [PluginService] public static IFramework              Framework       { get; private set; } = null!;
        [PluginService] public static ICondition              Conditions      { get; private set; } = null!;
        [PluginService] public static IGameGui                GameGui         { get; private set; } = null!;
        [PluginService] public static ITargetManager          Targets         { get; private set; } = null!;
        [PluginService] public static ITextureProvider        Textures        { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider    Interop         { get; private set; } = null!;
        [PluginService] public static IPluginLog              Log             { get; private set; } = null!;
        [PluginService] public static IDtrBar                 Dtr             { get; private set;} = null!;
        [PluginService] public static INotificationManager    Notifications   { get; private set;} = null!;
        // @formatter:on

    public static bool GetIcon(uint iconId, [NotNullWhen(true)] out IDalamudTextureWrap? icon)
    {
        if (iconId == 0)
        {
            icon = null;
            return false;
        }

        var tex = Textures.GetFromGameIcon(new GameIconLookup(iconId));
        return tex.TryGetWrap(out icon, out _);
    }
}
