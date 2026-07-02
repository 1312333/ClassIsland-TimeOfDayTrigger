using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace TimeTriggerPlugin;

[TriggerInfo("com.example.timetrigger", "每日指定时间", "\uE125")]
public class TimeOfDayTrigger : TriggerBase<TimeOfDayTriggerSettings>
{
    private readonly Dictionary<TimeOnly, Timer> _timers = new();
    private readonly ILogger<TimeOfDayTrigger> _logger;

    public TimeOfDayTrigger(ILogger<TimeOfDayTrigger> logger)
    {
        _logger = logger;
    }

    public override void Loaded()
    {
        _logger.LogInformation("⏰ 多时间点定时触发器已启动");
        Settings.PropertyChanged += SettingsOnPropertyChanged;
        ScheduleAllTriggers();
    }

    public override void UnLoaded()
    {
        Settings.PropertyChanged -= SettingsOnPropertyChanged;
        DisposeAllTimers();
    }

    private void SettingsOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DisposeAllTimers();
        ScheduleAllTriggers();
    }

    #region 定时器管理
    private void ScheduleAllTriggers()
    {
        if (Settings == null || !Settings.TriggerTimes.Any())
        {
            _logger.LogWarning("无有效触发时间点，跳过调度");
            return;
        }

        var now = DateTime.Now;
        foreach (var timePoint in Settings.TriggerTimes)
        {
            ScheduleSingleTrigger(timePoint, now);
        }
    }

    private void ScheduleSingleTrigger(TimeOnly timePoint, DateTime from)
    {
        var nextTriggerTime = GetNextTriggerTimeForTimePoint(timePoint, from);
        if (nextTriggerTime.HasValue)
        {
            var delay = nextTriggerTime.Value - from;
            if (delay < TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
            
            var timer = new Timer(_ => OnTrigger(timePoint), null, delay, Timeout.InfiniteTimeSpan);
            _timers[timePoint] = timer;
            
            _logger.LogInformation("时间点 {Time} 下次触发时间: {NextTime}", 
                timePoint.ToString("HH:mm"), nextTriggerTime.Value);
        }
    }

    private void DisposeAllTimers()
    {
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }
        _timers.Clear();
    }
    #endregion

    #region 时间计算
    private DateTime? GetNextTriggerTimeForTimePoint(TimeOnly timePoint, DateTime from)
    {
        var s = Settings;
        if (s == null) return null;

        if (IsDayMatch(from.DayOfWeek, s))
        {
            var todayTarget = new DateTime(from.Year, from.Month, from.Day, 
                timePoint.Hour, timePoint.Minute, 0);
            if (todayTarget > from) return todayTarget;
        }

        for (int i = 1; i <= 7; i++)
        {
            var date = from.AddDays(i);
            if (IsDayMatch(date.DayOfWeek, s))
            {
                return new DateTime(date.Year, date.Month, date.Day, 
                    timePoint.Hour, timePoint.Minute, 0);
            }
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
    #endregion

    private void OnTrigger(TimeOnly triggeredTimePoint)
    {
        _logger.LogInformation("⏰ 定时器触发：{Time}", triggeredTimePoint.ToString("HH:mm"));
        
        try
        {
            TriggerLogService.RecordTrigger(
                TimeSpan.FromHours(triggeredTimePoint.Hour) + TimeSpan.FromMinutes(triggeredTimePoint.Minute),
                true,
                $"多时间点触发：{triggeredTimePoint.ToString("HH:mm")}"
            );
        }
        catch
        {
            // 忽略日志记录错误
        }
        
        Trigger();
        
        var now = DateTime.Now;
        ScheduleSingleTrigger(triggeredTimePoint, now);
    }
}
