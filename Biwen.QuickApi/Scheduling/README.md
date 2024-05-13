任务调度ScheduleTask：
实现`IScheduleTask`接口，添加 ScheduleTask 特性，指定任务调度时间表达式，即可实现任务调度。

```csharp
/// <summary>
/// KeepAlive ScheduleTask
/// </summary>
/// <param name="logger"></param>
[ScheduleTask("* * * * *")] //每分钟一次
[ScheduleTask("0/3 * * * *")]//每3分钟执行一次
public class KeepAlive(ILogger<KeepAlive> logger) : IScheduleTask
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("keep alive!");
        await Task.CompletedTask;
    }
}
```

支持从Store中初始化任务调度列表，支持动态添加任务调度，支持动态删除任务调度。
```csharp
/// <summary>
/// Demo ScheduleTask，用于Store演示
/// </summary>
/// <param name="logger"></param>
public class DemoTask(ILogger<DemoTask> logger) : IScheduleTask
{
    public Task ExecuteAsync()
    {
        logger.LogInformation("Demo Schedule Done!");
        return Task.CompletedTask;
    }
}

/// <summary>
/// DemoStore演示
/// </summary>
public class DemoStore : IScheduleMetadaStore
{
    public Task<IEnumerable<ScheduleTaskMetadata>> GetAllAsync()
    {
        //模拟从数据库或配置文件中获取ScheduleTaskMetadata
        IEnumerable<ScheduleTaskMetadata> metadatas =
            [
                new ScheduleTaskMetadata(typeof(DemoTask), "* * * * *")
                {
                    Description="测试的Schedule"
                }
            ];

        return Task.FromResult(metadatas);
    }
}

```
最后注册DemoStore到DI容器中即可。
```csharp
builder.Services.AddScheduleMetadaStore<DemoStore>();
```

系统自带ConfigurationStore,appsettings.json配置如下

```json
{
  "BiwenQuickApi": {
    "Schedules": [
      {
        "ScheduleType": "Biwen.QuickApi.DemoWeb.Schedules.DemoConfigTask,Biwen.QuickApi.DemoWeb",
        "Cron": "0/5 * * * *",
        "Description": "Every 5 mins",
        "IsAsync": false,
        "IsStartOnInit": false
      },
      {
        "ScheduleType": "Biwen.QuickApi.DemoWeb.Schedules.DemoConfigTask,Biwen.QuickApi.DemoWeb",
        "Cron": "0/10 * * * *",
        "Description": "Every 10 mins",
        "IsAsync": false,
        "IsStartOnInit": false
      }
    ]
  }
}

```