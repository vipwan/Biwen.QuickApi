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