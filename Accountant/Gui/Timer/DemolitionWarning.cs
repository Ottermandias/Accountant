using Accountant.Classes;
using Accountant.Manager;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

namespace Accountant.Gui.Timer;

public class DemolitionWarning : IDisposable
{
    private readonly DemolitionManager _manager;
    private readonly IFramework        _framework;

    private DateTime _nextChange = DateTime.MinValue;

    private readonly List<(string Name, string Status, ColorId Color)> _warnings = [];
    public           ColorId                                           HeaderColor { get; private set; }
    private          Dictionary<PlotInfo, IActiveNotification>         _notifications = [];


    public IReadOnlyList<(string Name, string Status, ColorId Color)> Warnings
        => _warnings;

    public DemolitionWarning(DemolitionManager manager, IFramework framework)
    {
        _manager          =  manager;
        _framework        =  framework;
        _manager.Change   += OnChange;
        _framework.Update += OnFramework;
    }

    private void UpdateNextUpdate(DateTime now, TimeSpan timespan)
    {
        var timespanNextDay = TimeSpan.TicksPerDay - timespan.Ticks % TimeSpan.TicksPerDay;
        var nextUpdate      = now.AddTicks(timespanNextDay);
        if (nextUpdate < _nextChange)
            _nextChange = nextUpdate;
    }

    private void UpdateDisplay(PlotInfo plot, DemolitionManager.DemolitionInfo data, int days)
    {
        if (days < data.DisplayFrom)
            return;

        var status = days switch
        {
            > DemolitionManager.DefaultDisplayMax   => "Demolished",
            DemolitionManager.DefaultDisplayMax     => "<1 Day Remaining",
            DemolitionManager.DefaultDisplayMax - 1 => "1 Day Remaining",
            _                                       => $"{DemolitionManager.DefaultDisplayMax - days} Days Remaining",
        };
        var color = days > DemolitionManager.DefaultDisplayMax
            ? ColorId.TextCropWithered
            : days > data.DisplayWarningFrom
                ? ColorId.TextCropWilted
                : ColorId.TextCropGrowing;
        _warnings.Add((plot.Name, status, color));
        HeaderColor = (HeaderColor, color) switch
        {
            (ColorId.HeaderCropWithered, ColorId.TextCropWithered) => ColorId.HeaderCropWithered,
            (ColorId.HeaderCropWilted, ColorId.TextCropWithered)   => ColorId.HeaderCropWithered,
            (ColorId.HeaderCropGrowing, ColorId.TextCropWithered)  => ColorId.HeaderCropWithered,
            (ColorId.HeaderCropWithered, ColorId.TextCropWilted)   => ColorId.HeaderCropWithered,
            (ColorId.HeaderCropWilted, ColorId.TextCropWilted)     => ColorId.HeaderCropWilted,
            (ColorId.HeaderCropGrowing, ColorId.TextCropWilted)    => ColorId.HeaderCropWilted,
            (ColorId.HeaderCropWithered, ColorId.TextCropGrowing)  => ColorId.HeaderCropWithered,
            (ColorId.HeaderCropWilted, ColorId.TextCropGrowing)    => ColorId.HeaderCropWilted,
            (ColorId.HeaderCropGrowing, ColorId.TextCropGrowing)   => ColorId.HeaderCropGrowing,
            _                                                      => HeaderColor,
        };
    }

    private void UpdateNotifications(Dictionary<PlotInfo, IActiveNotification> notifications, PlotInfo plot,
        DemolitionManager.DemolitionInfo data, int days)
    {
        if (days < data.DisplayWarningFrom)
            return;

        var content = days switch
        {
            > DemolitionManager.DefaultDisplayMax =>
                $"Your house {plot.Name} may have been demolished. Please visit it with a tracked character or remove tracking if you do not own this house anymore.",
            DemolitionManager.DefaultDisplayMax =>
                $"Your house {plot.Name} will be demolished in less than a day. Please visit it with a tracked character.",
            DemolitionManager.DefaultDisplayMax - 1 =>
                $"Your house {plot.Name} will be demolished within the next day. Please visit it with a tracked character.",
            _ => $"Your house {plot.Name} will be demolished in {DemolitionManager.DefaultDisplayMax - days} days. Please visit it with a tracked character.",
        };

        if (!_notifications.Remove(plot, out var activeNotification))
        {
            var notification = new Notification
            {
                Title                       = "Housing Alert! Imminent Destruction!",
                Content                     = content,
                Icon                        = INotificationIcon.From(FontAwesomeIcon.ExclamationTriangle),
                Type                        = NotificationType.Warning,
                HardExpiry                  = DateTime.MaxValue,
                InitialDuration             = TimeSpan.MaxValue,
                ShowIndeterminateIfNoExpiry = true,
                Minimized                   = false,
                UserDismissable             = true,
                MinimizedText               = "Housing Alert! Imminent Destruction!",
            };
            notifications.Add(plot, Dalamud.Notifications.AddNotification(notification));
        }
        else
        {
            activeNotification.Content = content;
            notifications.Add(plot, activeNotification);
        }
    }

    private void OnFramework(IFramework framework)
    {
        var now = DateTime.UtcNow;
        if (_nextChange > now)
            return;

        _nextChange = DateTime.MaxValue;
        _warnings.Clear();
        var notifications = new Dictionary<PlotInfo, IActiveNotification>(_notifications.Count);
        HeaderColor = ColorId.HeaderCropGrowing;
        foreach (var (plot, data) in _manager.Data)
        {
            if (!data.Tracked)
                continue;

            var timespan = now - data.LastVisit;
            var days     = (int)(Math.Ceiling(timespan.TotalDays) + 0.5);
            UpdateNextUpdate(now, timespan);
            UpdateDisplay(plot, data, days);
            UpdateNotifications(notifications, plot, data, days);
        }

        DismissAll();
        _notifications = notifications;
    }

    private void OnChange()
        => _nextChange = DateTime.UtcNow;

    public void Dispose()
    {
        DismissAll();
        _manager.Change   -= OnChange;
        _framework.Update -= OnFramework;
    }

    private void DismissAll()
    {
        foreach (var notification in _notifications.Values)
            notification.DismissNow();
        _notifications.Clear();
    }
}
