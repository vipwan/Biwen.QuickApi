# 作业调度

Biwen.QuickApi 提供了灵活的作业调度系统，支持定时任务和后台作业。

## 基本概念

### 1. 作业定义

作业是一个实现 `IJob` 接口的类：

```csharp
public class CleanupJob : IJob
{
    private readonly ILogger<CleanupJob> _logger;

    public CleanupJob(ILogger<CleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting cleanup job");
        // 执行清理逻辑
        await Task.Delay(1000);
        _logger.LogInformation("Cleanup job completed");
    }
}
```

### 2. 调度配置

使用 `[Schedule]` 特性配置作业调度：

```csharp
[Schedule(Cron = "0 0 * * *")] // 每天午夜执行
public class DailyReportJob : IJob
{
    public async Task ExecuteAsync()
    {
        // 生成每日报告
    }
}
```

## 使用方式

### 1. 注册作业

在 `Program.cs` 中注册作业：

```csharp
builder.Services.AddJobs(Assembly.GetExecutingAssembly());
```

### 2. 配置调度

在 `appsettings.json` 中配置调度：

```json
{
  "Scheduling": {
    "Jobs": {
      "CleanupJob": {
        "Enabled": true,
        "Cron": "0 0 * * *"
      }
    }
  }
}
```

## 高级特性

### 1. 分布式调度

支持分布式环境下的作业调度：

```csharp
[Schedule(Cron = "0 0 * * *", Distributed = true)]
public class DistributedJob : IJob
{
    public async Task ExecuteAsync()
    {
        // 分布式作业逻辑
    }
}
```

### 2. 作业依赖

可以配置作业之间的依赖关系：

```csharp
[Schedule(Cron = "0 0 * * *", DependsOn = typeof(OtherJob))]
public class DependentJob : IJob
{
    public async Task ExecuteAsync()
    {
        // 依赖其他作业的作业逻辑
    }
}
```

### 3. 作业状态

可以获取和监控作业状态：

```csharp
public class JobStatusApi : BaseQuickApi
{
    private readonly IJobManager _jobManager;

    public JobStatusApi(IJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public async Task<JobStatus> GetStatus(string jobName)
    {
        return await _jobManager.GetJobStatusAsync(jobName);
    }
}
```

## 最佳实践

1. **作业设计**
   - 保持作业职责单一
   - 避免长时间运行的作业
   - 使用异步方法

2. **错误处理**
   - 实现适当的错误处理
   - 记录作业执行日志
   - 配置重试策略

3. **性能考虑**
   - 避免资源密集型作业
   - 考虑使用分布式调度
   - 监控作业执行时间

## 示例

### 1. 简单的定时任务

```csharp
[Schedule(Cron = "*/5 * * * *")]
public class SimpleJob : IJob
{
    public async Task ExecuteAsync()
    {
        Console.WriteLine("Simple job executed at " + DateTime.Now);
    }
}
```

### 2. 带参数的作业

```csharp
public class ParameterizedJob : IJob
{
    private readonly JobOptions _options;

    public ParameterizedJob(IOptions<JobOptions> options)
    {
        _options = options.Value;
    }

    public async Task ExecuteAsync()
    {
        // 使用配置参数
        Console.WriteLine($"Job executed with parameter: {_options.Parameter}");
    }
}
```

## 下一步

- [发布订阅](EventPublishing.md)
- [UnitOfWork](UnitOfWork.md)
- [审计](Auditing.md) 