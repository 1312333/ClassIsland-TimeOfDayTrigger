using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using System.Linq;
using System.Text.RegularExpressions;

namespace TimeTriggerPlugin;

public class TimeOfDayTriggerSettingsControl : TriggerSettingsControlBase<TimeOfDayTriggerSettings>
{
    private ListBox? _timeListBox;
    private TimePicker? _timePicker;
    private TextBox? _importTextBox;
    private TextBlock? _importHint;
    private CheckBox? _monCheck, _tueCheck, _wedCheck, _thuCheck, _friCheck, _satCheck, _sunCheck;
    private bool _isUpdating;

    public TimeOfDayTriggerSettingsControl()
    {
        BuildUi();
        Dispatcher.UIThread.Post(InitializeFromSettings);
    }

    private void InitializeFromSettings()
    {
        if (Settings == null) return;
        UpdateUiFromSettings();
    }

    private void BuildUi()
    {
        var mainPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(12),
            Spacing = 20
        };

        // ========== 1. 时间点管理区 ==========
        var timeLabel = new TextBlock
        {
            Text = "触发时间点",
            FontWeight = FontWeight.SemiBold,
            FontSize = 15
        };
        mainPanel.Children.Add(timeLabel);

        var timeEditPanel = new StackPanel { Spacing = 8 };
        
        _timePicker = new TimePicker
        {
            Height = 36
        };
        timeEditPanel.Children.Add(_timePicker);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };
        var addButton = new Button { Content = "添加时间点", Width = 100 };
        addButton.Click += (_, _) => AddTimePoint();
        buttonPanel.Children.Add(addButton);

        var removeButton = new Button { Content = "删除选中", Width = 100 };
        removeButton.Click += (_, _) => RemoveSelectedTimePoint();
        buttonPanel.Children.Add(removeButton);
        
        timeEditPanel.Children.Add(buttonPanel);

        _timeListBox = new ListBox
        {
            Height = 120,
            Margin = new Avalonia.Thickness(0, 8, 0, 0)
        };
        timeEditPanel.Children.Add(_timeListBox);
        mainPanel.Children.Add(timeEditPanel);

        // ========== 2. 便携导入区 ==========
        var importLabel = new TextBlock
        {
            Text = "便携导入（粘贴文本自动识别时间）",
            FontWeight = FontWeight.SemiBold,
            FontSize = 15
        };
        mainPanel.Children.Add(importLabel);

        var importPanel = new StackPanel { Spacing = 8 };
        
        _importTextBox = new TextBox
        {
            AcceptsReturn = true,
            Height = 80,
            FontSize = 12,
            Watermark = "粘贴包含时间的文本，例如：\n08:35 - 08:45 早会\n08:50 - 09:40 语文"
        };
        importPanel.Children.Add(_importTextBox);

        var importButton = new Button
        {
            Content = "识别并添加时间点",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        importButton.Click += (_, _) => ImportFromText();
        importPanel.Children.Add(importButton);

        _importHint = new TextBlock
        {
            FontSize = 12,
            Foreground = Brushes.Gray,
            TextWrapping = TextWrapping.Wrap
        };
        importPanel.Children.Add(_importHint);
        
        mainPanel.Children.Add(importPanel);

        // ========== 3. 星期选择区 ==========
        var weekLabel = new TextBlock
        {
            Text = "触发星期",
            FontWeight = FontWeight.SemiBold,
            FontSize = 15
        };
        mainPanel.Children.Add(weekLabel);

        var weekPanel = new WrapPanel { Orientation = Orientation.Horizontal };
        _monCheck = CreateWeekCheckBox("周一");
        _tueCheck = CreateWeekCheckBox("周二");
        _wedCheck = CreateWeekCheckBox("周三");
        _thuCheck = CreateWeekCheckBox("周四");
        _friCheck = CreateWeekCheckBox("周五");
        _satCheck = CreateWeekCheckBox("周六");
        _sunCheck = CreateWeekCheckBox("周日");
        
        weekPanel.Children.Add(_monCheck);
        weekPanel.Children.Add(_tueCheck);
        weekPanel.Children.Add(_wedCheck);
        weekPanel.Children.Add(_thuCheck);
        weekPanel.Children.Add(_friCheck);
        weekPanel.Children.Add(_satCheck);
        weekPanel.Children.Add(_sunCheck);
        mainPanel.Children.Add(weekPanel);

        var scrollViewer = new ScrollViewer { Content = mainPanel };
        Content = scrollViewer;
    }

    private CheckBox CreateWeekCheckBox(string content)
    {
        var checkBox = new CheckBox
        {
            Content = content,
            Margin = new Avalonia.Thickness(0, 6, 20, 6),
            FontSize = 14
        };
        checkBox.IsCheckedChanged += WeekCheckOnChanged;
        return checkBox;
    }

    #region 时间点增删操作
    private void AddTimePoint()
    {
        if (_isUpdating || Settings == null || _timePicker == null) return;
        
        _isUpdating = true;
        try
        {
            var selectedTime = TimeOnly.FromTimeSpan(_timePicker.SelectedTime ?? TimeSpan.FromHours(8));
            if (!Settings.TriggerTimes.Contains(selectedTime))
            {
                Settings.TriggerTimes.Add(selectedTime);
                RefreshTimeList();
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void RemoveSelectedTimePoint()
    {
        if (_isUpdating || Settings == null || _timeListBox == null) return;
        
        _isUpdating = true;
        try
        {
            if (_timeListBox.SelectedItem is TextBlock item && 
                TimeOnly.TryParse(item.Text, out var time))
            {
                Settings.TriggerTimes.Remove(time);
                RefreshTimeList();
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void RefreshTimeList()
    {
        if (Settings == null || _timeListBox == null) return;
        
        _timeListBox.ItemsSource = Settings.TriggerTimes
            .OrderBy(t => t)
            .Select(t => new TextBlock { Text = t.ToString("HH:mm"), FontSize = 14 })
            .ToList();
    }
    #endregion

    #region 文本识别导入
    private void ImportFromText()
    {
        if (_isUpdating || Settings == null || _importTextBox == null) return;
        var input = _importTextBox.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            _importHint!.Text = "请先粘贴包含时间的文本";
            return;
        }

        _isUpdating = true;
        try
        {
            var timeRegex = new Regex(@"\b([01]?\d|2[0-3]):([0-5]\d)\b");
            var matches = timeRegex.Matches(input);

            var extractedTimes = new HashSet<TimeOnly>();
            foreach (Match match in matches)
            {
                if (TimeOnly.TryParse(match.Value, out var time))
                {
                    extractedTimes.Add(time);
                }
            }

            if (extractedTimes.Count == 0)
            {
                _importHint!.Text = "未识别到有效时间，请检查文本格式";
                return;
            }

            var addCount = 0;
            foreach (var time in extractedTimes)
            {
                if (!Settings.TriggerTimes.Contains(time))
                {
                    Settings.TriggerTimes.Add(time);
                    addCount++;
                }
            }

            RefreshTimeList();
            _importHint!.Text = $"识别完成：共找到 {extractedTimes.Count} 个时间点，新增 {addCount} 个（已自动去重）";
        }
        finally
        {
            _isUpdating = false;
        }
    }
    #endregion

    #region 通用事件与更新
    private void WeekCheckOnChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isUpdating || Settings == null) return;
        _isUpdating = true;
        try
        {
            Settings.IsMonday = _monCheck?.IsChecked == true;
            Settings.IsTuesday = _tueCheck?.IsChecked == true;
            Settings.IsWednesday = _wedCheck?.IsChecked == true;
            Settings.IsThursday = _thuCheck?.IsChecked == true;
            Settings.IsFriday = _friCheck?.IsChecked == true;
            Settings.IsSaturday = _satCheck?.IsChecked == true;
            Settings.IsSunday = _sunCheck?.IsChecked == true;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateUiFromSettings()
    {
        if (Settings == null) return;
        
        _isUpdating = true;
        try
        {
            RefreshTimeList();
            if (_monCheck != null) _monCheck.IsChecked = Settings.IsMonday;
            if (_tueCheck != null) _tueCheck.IsChecked = Settings.IsTuesday;
            if (_wedCheck != null) _wedCheck.IsChecked = Settings.IsWednesday;
            if (_thuCheck != null) _thuCheck.IsChecked = Settings.IsThursday;
            if (_friCheck != null) _friCheck.IsChecked = Settings.IsFriday;
            if (_satCheck != null) _satCheck.IsChecked = Settings.IsSaturday;
            if (_sunCheck != null) _sunCheck.IsChecked = Settings.IsSunday;
        }
        finally
        {
            _isUpdating = false;
        }
    }
    #endregion
}
