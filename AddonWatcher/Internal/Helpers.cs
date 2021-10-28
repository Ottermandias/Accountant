using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AddonWatcher.Internal;

public static class Helpers
{
    public static unsafe SeString TextNodeToString(AtkTextNode* node)
        => MemoryHelper.ReadSeString(&node->NodeText);
}
