using System;
using System.Threading;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace TimeTriggerPlugin;

[TriggerInfo("com.example.timetrigger", "每日指定时间", "\uE125")]
public class TimeOfDayTrigger : TriggerBase<TimeOfDayTriggerSettings>
{
    private Timer? _timer;
    private readonly ILogger<TimeOfDayTrigger> _logger;

    public TimeOfDayTrigger(ILogger<TimeOfDayTrigger> logger)
    {
        _logger = logger;
    }

    public override void Loaded()
    {
        _logger.LogInformation("⏰ 定时触发器已启动");
        Settings.PropertyChanged += SettingsOnPropertyChanged;
        ScheduleNextTrigger();
    }

    public override void UnLoaded()
    {
        Settings.PropertyChanged -= SettingsOnPropertyChanged;
        _timer?.Dispose();
        _timer = null;
    }

    private void SettingsOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _timer?.Dispose();
        _timer = null;
        ScheduleNextTrigger();
    }

    private void ScheduleNextTrigger()
    {
        var now = DateTime.Now;
        var target = GetNextTriggerTime(now);
        if (target.HasValue)
        {
            var delay = target.Value - now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
            _timer = new Timer(_ => OnTrigger(), null, delay, Timeout.InfiniteTimeSpan);
            _logger.LogInformation("下次触发时间: {Time}", target.Value);
        }
    }

    private DateTime? GetNextTriggerTime(DateTime from)
    {
        var s = Settings;
        if (s == null) return null;

        if (IsDayMatch(from.DayOfWeek, s))
        {
            var today = new DateTime(from.Year, from.Month, from.Day, s.Hour, s.Minute, 0);
            if (today > from) return today;
        }

        for (int i = 1; i <= 7; i++)
        {
            var date = from.AddDays(i);
            if (IsDayMatch(date.DayOfWeek, s))
                return new DateTime(date.Year, date.Month, date.Day, s.Hour, s.Minute, 0);
        }
        return null;
    }

    private bool IsDayMatch(DayOfWeek day, TimeOfDayTriggerSettings s) => day switch
    {
        DayOfWeek.Monday => s.IsMonday,
        DayOfWeek.Tuesday => s.IsTuesday,
        DayOfWeek.Wednesday => s.IsWednesday,
        DayOfWeek.Thursday => s.IsThursday,
        DayOfWeek.Friday => s.IsFriday,
        DayOfWeek.Saturday => s.IsSaturday,
        DayOfWeek.Sunday => s.IsSunday,
        _ => false
    };

    private void OnTrigger()
    {
        _logger.LogInformation("⏰ 定时器触发了！");
        
        // 记录触发日志
        try
        {
            TriggerLogService.RecordTrigger(
                TimeSpan.FromHours(Settings?.Hour ?? 0) + TimeSpan.FromMinutes(Settings?.Minute ?? 0),
                true,
                "触发成功"
            );
        }
        catch
        {
            // 忽略日志记录错误
        }
        
        Trigger();
        ScheduleNextTrigger();
    }
}
