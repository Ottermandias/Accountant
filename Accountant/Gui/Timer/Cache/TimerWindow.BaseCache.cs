using System;
using System.Collections.Generic;
using System.Numerics;
using Accountant.Gui.Helper;
using Accountant.Timers;
using ImGuiNET;

namespace Accountant.Gui.Timer;

public partial class TimerWindow
{
    public partial class BaseCache
    {
        protected readonly TimerWindow       Window;
        protected readonly List<CacheObject> Objects    = new();
        protected readonly List<SmallHeader> Headers    = new();
        private readonly   HashSet<string>   _seenNames = new();

        protected DateTime Now;
        private   DateTime _nextChange = DateTime.MinValue;

        public           ColorId     Color       = ColorId.NeutralHeader;
        protected        DateTime    DisplayTime = DateTime.MaxValue;
        private readonly ConfigFlags _requiredFlags;
        public readonly  string      Name;

        protected BaseCache(string name, ConfigFlags requiredFlags, TimerWindow window)
        {
            Name           = name;
            _requiredFlags = requiredFlags;
            Window         = window;
        }

        protected DateTime UpdateNextChange(DateTime time)
        {
            if (time < _nextChange && time > Now)
                _nextChange = time;

            return time;
        }

        protected string GetName(string name, uint serverId)
        {
            if (_seenNames.Add(name))
                return name;

            var server = Accountant.GameData.GetWorldName(serverId);
            return $"{name} ({server})";
        }

        public void Update(DateTime now)
        {
            if (now <= _nextChange)
                return;

            Now         = now;
            Color       = ColorId.NeutralHeader;
            _nextChange = DateTime.MaxValue;
            _seenNames.Clear();
            Objects.Clear();
            Headers.Clear();

            UpdateInternal();
        }

        protected virtual void UpdateInternal()
        { }

        public void Resetter()
            => _nextChange = DateTime.UtcNow;

        public void Draw(DateTime now)
        {
            if (!Accountant.Config.Flags.Check(_requiredFlags))
                return;

            Update(now);
            if (Headers.Count == 0)
                return;

            using var id     = ImGuiRaii.PushId(Name);
            using var c      = ImGuiRaii.PushColor(ImGuiCol.Header, Color.Value());
            var       posY   = ImGui.GetCursorPosY();
            var       header = ImGui.CollapsingHeader(Name);
            c.Pop();
            if (DisplayTime > now && DisplayTime != DateTime.MaxValue)
            {
                var s     = TimeSpanString(DisplayTime - now);
                var width = ImGui.CalcTextSize(s).X;
                var pos   = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - width, posY));
                ImGui.AlignTextToFramePadding();
                ImGui.Text(s);
                ImGui.SetCursorPos(pos);
            }

            if (!header)
                return;

            foreach (var smallHeader in Headers)
                smallHeader.Draw(this, now);
        }
    }
}
