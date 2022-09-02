using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AddonWatcher.Internal;

public static class Helpers
{
    public static unsafe SeString TextNodeToString(AtkTextNode* node)
        => node->AtkResNode.Type == NodeType.Text 
            ? MemoryHelper.ReadSeString(&node->NodeText) 
            : SeString.Empty;
}
