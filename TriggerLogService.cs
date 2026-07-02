using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TimeTriggerPlugin;

public class TriggerLogEntry
{
    public DateTime TriggerTime { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public static class TriggerLogService
{
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ClassIsland",
        "Plugins",
        "TimeTriggerPlugin",
        "trigger_logs.json"
    );

    private static readonly object _lock = new();
    private static List<TriggerLogEntry>? _logs;

    public static List<TriggerLogEntry> GetLogs()
    {
        lock (_lock)
        {
            if (_logs == null)
            {
                LoadLogs();
            }
            return _logs ?? new List<TriggerLogEntry>();
        }
    }

    public static void RecordTrigger(TimeSpan scheduledTime, bool success = true, string? message = null)
    {
        lock (_lock)
        {
            if (_logs == null)
            {
                LoadLogs();
            }
            _logs?.Insert(0, new TriggerLogEntry
            {
                TriggerTime = DateTime.Now,
                ScheduledTime = scheduledTime,
                Success = success,
                Message = message
            });
            if (_logs?.Count > 100)
            {
                _logs = _logs.Take(100).ToList();
            }
            SaveLogs();
        }
    }

    public static void ClearLogs()
    {
        lock (_lock)
        {
            _logs = new List<TriggerLogEntry>();
            SaveLogs();
        }
    }

    private static void LoadLogs()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                var json = File.ReadAllText(LogFilePath);
                _logs = JsonSerializer.Deserialize<List<TriggerLogEntry>>(json) ?? new List<TriggerLogEntry>();
            }
            else
            {
                _logs = new List<TriggerLogEntry>();
            }
        }
        catch
        {
            _logs = new List<TriggerLogEntry>();
        }
    }

    private static void SaveLogs()
    {
        try
        {
            var dir = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var json = JsonSerializer.Serialize(_logs, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(LogFilePath, json);
        }
        catch
        {
            // 忽略保存错误
        }
    }
}
