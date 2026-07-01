using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TimeTriggerPlugin;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 注册每日定时触发器
        services.AddTrigger<TimeOfDayTrigger, TimeOfDayTriggerSettingsControl>();
        
        // 注册设置页面（触发日志与批量导入）
        services.AddSettingsPage<TimeTriggerSettingsPage>();
    }
}
