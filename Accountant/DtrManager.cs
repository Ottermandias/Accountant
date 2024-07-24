using System;
using Accountant.Gui;
using Accountant.Gui.Timer;
using Accountant.Internal;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;

namespace Accountant;

internal class DtrManager(
    IDtrBar dtr,
    TimerWindow.CropCache cropCache,
    TimerWindow.RetainerCache retainerCache,
    TimerWindow.MachineCache airshipCache,
    TimerWindow.MachineCache subCache)
{
    private const string        DtrName = "Accountant.DTR";
    private       bool          _enabled;
    private       IDtrBarEntry? _entry;

    private const ushort AvailableColor = 32;
    //private const ushort FinishedColor  = 45;
    private const ushort FinishedColor = 46;
    private const ushort WiltingColor  = 541;
    private const ushort BrokenColor   = 17;

    public void Enable()
    {
        if (_enabled)
            return;

        _enabled       = true;
        _entry         = dtr.Get(DtrName);
        _entry.OnClick = TimerWindow.Toggle;
        _entry.Tooltip = "Click to toggle Accountant Timer Window.";
        Update(true);
    }

    public void Disable()
    {
        if (!_enabled)
            return;

        _enabled = false;
        _entry   = null;
        dtr.Remove(DtrName);
    }

    internal void Update(bool updateCaches)
    {
        if (!Accountant.Config.ShowDtr || _entry == null)
            return;

        var now     = DateTime.UtcNow;
        var builder = new SeStringBuilder();

        if (Accountant.Config.EnableRetainers)
        {
            if (updateCaches)
                retainerCache.Update(now);
            PushColor(builder, retainerCache.Counter.GetHeader(SeIconChar.BoxedLetterR), retainerCache.Color);
            if (Accountant.Config.EnableRetainersDtrTimer)
            {
                var timeLeft = Helpers.TimeLeftFromNow(retainerCache.Counter.GetTime());
                if (TimeSpan.Compare(timeLeft, TimeSpan.Zero) > 0)
                {
                    PushColor(builder, $" ({Helpers.FormatTimeSpan(timeLeft)})", retainerCache.Color);
                }
            }
        }

        if (Accountant.Config.EnableSubmersibles)
        {
            if (updateCaches)
                subCache.Update(now);

            if (Accountant.Config.EnableRetainers)
                builder.AddText(" ");

            if (Accountant.Config.EnableAirships)
            {
                if (updateCaches)
                    airshipCache.Update(now);
                PushColor(builder,
                    $"{SeIconChar.BoxedLetterM.ToIconChar()}{airshipCache.GlobalCounter.Completed + subCache.GlobalCounter.Completed}|"
                  + $"{airshipCache.GlobalCounter.Available + subCache.GlobalCounter.Available}|"
                  + $"{airshipCache.GlobalCounter.Sent + subCache.GlobalCounter.Sent}",
                    airshipCache.Color.Combine(subCache.Color));
            }
            else
            {
                PushColor(builder, subCache.GlobalCounter.GetHeader(SeIconChar.BoxedLetterM), subCache.Color);
            }
        }
        else if (Accountant.Config.EnableAirships)
        {
            if (Accountant.Config.EnableRetainers)
                builder.AddText(" ");

            if (updateCaches)
                airshipCache.Update(now);
            PushColor(builder, airshipCache.GlobalCounter.GetHeader(SeIconChar.BoxedLetterM), airshipCache.Color);
        }

        if (Accountant.Config.EnableCrops)
        {
            if (Accountant.Config.EnableRetainers || Accountant.Config.EnableSubmersibles || Accountant.Config.EnableAirships)
                builder.AddText(" ");

            if (updateCaches)
                cropCache.Update(now);
            PushColor(builder, SeIconChar.BoxedLetterC.ToIconString(), cropCache.Color);
        }

        _entry.Text = builder.BuiltString;
    }

    private static void PushColor(SeStringBuilder builder, string text, ColorId color)
    {
        ushort uiColor = color switch
        {
            ColorId.HeaderObjectsAway    => 0,
            ColorId.HeaderObjectsMixed   => AvailableColor,
            ColorId.HeaderObjectsHome    => FinishedColor,
            ColorId.TextCropWithered     => BrokenColor,
            ColorId.TextCropWilted       => WiltingColor,
            ColorId.TextCropGrown        => FinishedColor,
            ColorId.TextCropGrowing      => 0,
            ColorId.TextCropGuaranteed   => 0,
            ColorId.HeaderCropWithered   => BrokenColor,
            ColorId.HeaderCropWilted     => WiltingColor,
            ColorId.HeaderCropGrown      => FinishedColor,
            ColorId.HeaderCropGrowing    => 0,
            ColorId.HeaderCropGuaranteed => 0,
            ColorId.TextObjectsAway      => 0,
            ColorId.TextObjectsMixed     => AvailableColor,
            ColorId.TextObjectsHome      => FinishedColor,
            _                            => 0,
        };
        if (uiColor != 0)
            builder.AddUiGlow(uiColor);
        builder.AddText(text);
        if (uiColor != 0)
            builder.AddUiGlowOff();
    }
}
