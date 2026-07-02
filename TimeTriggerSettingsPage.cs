using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;

namespace TimeTriggerPlugin;

[SettingsPageInfo("com.example.timetrigger.settings", "定时触发器设置")]
public class TimeTriggerSettingsPage : SettingsPageBase
{
    private TextBlock? _statsText;
    private ItemsControl? _logList;
    private TextBox? _importTextBox;
    private readonly ObservableCollection<TextBlock> _logItems = new();

    public TimeTriggerSettingsPage()
    {
        BuildUi();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RefreshLogs();
    }

    private void BuildUi()
    {
        var scrollViewer = new ScrollViewer();
        var mainPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 16
        };

        // 标题
        mainPanel.Children.Add(new TextBlock
        {
            Text = "⏰ 每日定时触发器",
            FontSize = 24,
            FontWeight = FontWeight.Bold
        });

        // 统计信息
        mainPanel.Children.Add(CreateSectionHeader("触发统计"));
        _statsText = new TextBlock
        {
            Text = "加载中...",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(8, 0, 0, 0)
        };
        mainPanel.Children.Add(_statsText);

        // 触发日志
        mainPanel.Children.Add(CreateSectionHeader("触发日志（最近 20 条）"));
        
        var logPanel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(8, 0, 0, 0) };
        
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };
        
        var refreshButton = new Button
        {
            Content = "刷新日志"
        };
        refreshButton.Click += (s, e) => RefreshLogs();
        buttonPanel.Children.Add(refreshButton);

        var clearButton = new Button
        {
            Content = "清空日志"
        };
        clearButton.Click += (s, e) =>
        {
            TriggerLogService.ClearLogs();
            RefreshLogs();
        };
        buttonPanel.Children.Add(clearButton);

        logPanel.Children.Add(buttonPanel);

        _logList = new ItemsControl
        {
            ItemsSource = _logItems
        };
        logPanel.Children.Add(_logList);
        mainPanel.Children.Add(logPanel);

        // 批量导入时间点
        mainPanel.Children.Add(CreateSectionHeader("批量导入时间点（每行一个时间，格式 HH:mm）"));
        var importPanel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(8, 0, 0, 0) };
        _importTextBox = new TextBox
        {
            AcceptsReturn = true,
            Height = 120,
            Watermark = "示例：\n08:00\n12:00\n18:00"
        };
        importPanel.Children.Add(_importTextBox);

        var importButton = new Button
        {
            Content = "解析时间点",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        importButton.Click += OnImportClick;
        importPanel.Children.Add(importButton);

        var importHint = new TextBlock
        {
            Text = "提示：解析后可复制时间点，在自动化触发器设置中通过「便携导入」功能批量添加到单个触发器中。\n一个触发器支持多个时间点，无需重复创建多个自动化。",
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.Gray,
            FontSize = 12
        };
        importPanel.Children.Add(importHint);
        mainPanel.Children.Add(importPanel);

        scrollViewer.Content = mainPanel;
        Content = scrollViewer;
    }

    private static Control CreateSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            Margin = new Avalonia.Thickness(0, 8, 0, 4)
        };
    }

    private void RefreshLogs()
    {
        var logs = TriggerLogService.GetLogs();
        
        var totalCount = logs.Count;
        var todayCount = logs.Count(l => l.TriggerTime.Date == DateTime.Today);
        var successCount = logs.Count(l => l.Success);
        
        if (_statsText != null)
        {
            _statsText.Text = $"总触发次数：{totalCount}\n今日触发次数：{todayCount}\n成功次数：{successCount}\n成功率：{(totalCount > 0 ? (successCount * 100.0 / totalCount).ToString("F1") : "N/A")}%";
        }

        if (_logItems != null)
        {
            _logItems.Clear();
            var recentLogs = logs.Take(20).ToList();
            
            foreach (var log in recentLogs)
            {
                var status = log.Success ? "✅" : "❌";
                var timeStr = log.TriggerTime.ToString("yyyy-MM-dd HH:mm:ss");
                var scheduledStr = $"{log.ScheduledTime.Hours:D2}:{log.ScheduledTime.Minutes:D2}";
                _logItems.Add(new TextBlock
                {
                    Text = $"{status} {timeStr} (计划 {scheduledStr}) - {log.Message}",
                    FontSize = 12,
                    Margin = new Avalonia.Thickness(0, 2, 0, 2)
                });
            }
            if (_logItems.Count == 0)
            {
                _logItems.Add(new TextBlock
                {
                    Text = "暂无触发记录",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyle.Italic
                });
            }
        }
    }

    private void OnImportClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_importTextBox == null || string.IsNullOrWhiteSpace(_importTextBox.Text))
            return;

        var lines = _importTextBox.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var successCount = 0;
        var failCount = 0;
        var importedTimes = new List<string>();

        foreach (var line in lines)
        {
            if (TimeOnly.TryParse(line, out var time))
            {
                importedTimes.Add(time.ToString("HH:mm"));
                successCount++;
            }
            else
            {
                failCount++;
            }
        }

        var resultText = $"解析完成：成功 {successCount} 个，失败 {failCount} 个\n\n" +
                         "已解析的时间点：\n" + string.Join("\n", importedTimes.OrderBy(t => t)) + "\n\n" +
                         "使用方法：\n" +
                         "1. 复制上面的时间点文本\n" +
                         "2. 进入「自动化」面板，创建或编辑定时触发器\n" +
                         "3. 在「便携导入」区域粘贴文本，点击「识别并添加时间点」\n" +
                         "4. 所有时间点会添加到同一个触发器中";

        if (_statsText != null)
        {
            _statsText.Text = resultText;
        }
    }
}
