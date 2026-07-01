# 每日定时触发器插件 v1.2.4

## 项目简介
面向 ClassIsland 2.0+ 的自动化触发器插件，支持自定义每天指定时分触发，可选择星期。

## 功能特性
- ✅ 每日指定时间触发
- ✅ 星期筛选（周一至周日自由选择）
- ✅ 设置持久化保存
- ✅ 触发日志与统计
- ✅ 批量导入时间（设置页面）
- ✅ 独立设置页面

## 编译方法

### 环境要求
- .NET 8.0 SDK
- ClassIsland 2.0+

### 编译步骤
```bash
dotnet build -c Release
```

### 打包插件
编译完成后，将以下文件打包为 zip，改扩展名为 .cipx：
- TimeTriggerPlugin.dll
- manifest.yml

## 文件说明
- `TimeOfDayTriggerSettings.cs` - 触发器设置模型
- `TimeOfDayTrigger.cs` - 触发器主逻辑
- `TimeOfDayTriggerSettingsControl.cs` - 触发器设置控件
- `Plugin.cs` - 插件入口
- `TriggerLogService.cs` - 触发日志服务
- `TimeTriggerSettingsPage.cs` - 设置页面
- `TimeTriggerPlugin.csproj` - 项目文件
- `manifest.yml` - 插件清单

## 版本历史
### v1.2.4
- 改用 TextBox 输入时间（参考官方 Cron 触发器实现）
- 优化界面大小和间距
- 添加输入验证和自动格式化
- 修复 FormatException 崩溃 bug

### v1.2.0
- 新增触发日志与统计功能
- 新增批量导入时间功能
- 新增设置页面

### v1.1.0
- 新增星期筛选功能

### v1.0.0
- 初始版本
