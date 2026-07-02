using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace TimeTriggerPlugin;

public class TimeOfDayTriggerSettings : ObservableRecipient
{
    private ObservableCollection<TimeOnly> _triggerTimes = new() { new TimeOnly(8, 0) };
    private bool _isMonday = true;
    private bool _isTuesday = true;
    private bool _isWednesday = true;
    private bool _isThursday = true;
    private bool _isFriday = true;
    private bool _isSaturday = false;
    private bool _isSunday = false;

    public ObservableCollection<TimeOnly> TriggerTimes
    {
        get => _triggerTimes;
        set => SetProperty(ref _triggerTimes, value);
    }

    public bool IsMonday
    {
        get => _isMonday;
        set => SetProperty(ref _isMonday, value);
    }
    public bool IsTuesday
    {
        get => _isTuesday;
        set => SetProperty(ref _isTuesday, value);
    }
    public bool IsWednesday
    {
        get => _isWednesday;
        set => SetProperty(ref _isWednesday, value);
    }
    public bool IsThursday
    {
        get => _isThursday;
        set => SetProperty(ref _isThursday, value);
    }
    public bool IsFriday
    {
        get => _isFriday;
        set => SetProperty(ref _isFriday, value);
    }
    public bool IsSaturday
    {
        get => _isSaturday;
        set => SetProperty(ref _isSaturday, value);
    }
    public bool IsSunday
    {
        get => _isSunday;
        set => SetProperty(ref _isSunday, value);
    }

    public override string ToString()
    {
        var days = new List<string>();
        if (IsMonday) days.Add("一");
        if (IsTuesday) days.Add("二");
        if (IsWednesday) days.Add("三");
        if (IsThursday) days.Add("四");
        if (IsFriday) days.Add("五");
        if (IsSaturday) days.Add("六");
        if (IsSunday) days.Add("日");

        var dayStr = days.Count == 7 ? "每天" : string.Join("、", days);
        var timeStr = TriggerTimes.Any() 
            ? string.Join("、", TriggerTimes.OrderBy(t => t).Select(t => t.ToString("HH:mm"))) 
            : "无时间点";
        
        return $"{dayStr} {timeStr}";
    }
}
