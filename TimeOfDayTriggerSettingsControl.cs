using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;

namespace TimeTriggerPlugin;

public class TimeOfDayTriggerSettingsControl : TriggerSettingsControlBase<TimeOfDayTriggerSettings>
{
    private TextBox? _hourTextBox;
    private TextBox? _minuteTextBox;
    private CheckBox? _monCheck, _tueCheck, _wedCheck, _thuCheck, _friCheck, _satCheck, _sunCheck;
    private bool _isUpdating;

    public TimeOfDayTriggerSettingsControl()
    {
        BuildUi();
        // 延迟加载，确保 Settings 已经被设置
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
            Spacing = 16
        };

        // 时间选择
        var timeLabel = new TextBlock
        {
            Text = "触发时间",
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            FontSize = 15
        };
        mainPanel.Children.Add(timeLabel);

        var timePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        _hourTextBox = new TextBox
        {
            Width = 70,
            Height = 36,
            FontSize = 16,
            Watermark = "时",
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };
        _hourTextBox.LostFocus += HourTextBoxOnLostFocus;
        timePanel.Children.Add(_hourTextBox);

        var colonText = new TextBlock
        {
            Text = ":",
            FontSize = 22,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 4)
        };
        timePanel.Children.Add(colonText);

        _minuteTextBox = new TextBox
        {
            Width = 70,
            Height = 36,
            FontSize = 16,
            Watermark = "分",
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };
        _minuteTextBox.LostFocus += MinuteTextBoxOnLostFocus;
        timePanel.Children.Add(_minuteTextBox);

        mainPanel.Children.Add(timePanel);

        // 星期选择
        var weekLabel = new TextBlock
        {
            Text = "触发星期",
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            FontSize = 15
        };
        mainPanel.Children.Add(weekLabel);

        var weekPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal
        };

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

        Content = mainPanel;
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

    private void HourTextBoxOnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isUpdating || Settings == null || _hourTextBox == null) return;
        _isUpdating = true;
        try
        {
            if (int.TryParse(_hourTextBox.Text, out var hour))
            {
                hour = Math.Clamp(hour, 0, 23);
                Settings.Hour = hour;
                _hourTextBox.Text = hour.ToString("D2");
            }
            else
            {
                _hourTextBox.Text = Settings.Hour.ToString("D2");
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void MinuteTextBoxOnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isUpdating || Settings == null || _minuteTextBox == null) return;
        _isUpdating = true;
        try
        {
            if (int.TryParse(_minuteTextBox.Text, out var minute))
            {
                minute = Math.Clamp(minute, 0, 59);
                Settings.Minute = minute;
                _minuteTextBox.Text = minute.ToString("D2");
            }
            else
            {
                _minuteTextBox.Text = Settings.Minute.ToString("D2");
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateUiFromSettings()
    {
        if (Settings == null || _hourTextBox == null || _minuteTextBox == null) return;
        _isUpdating = true;
        try
        {
            _hourTextBox.Text = Settings.Hour.ToString("D2");
            _minuteTextBox.Text = Settings.Minute.ToString("D2");

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
}
