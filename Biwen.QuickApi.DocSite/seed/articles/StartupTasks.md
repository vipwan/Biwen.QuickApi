# 启动任务

Biwen.QuickApi 提供了启动任务功能，用于在应用程序启动时执行初始化操作。

## 基本概念

### 1. 启动任务定义

实现 `IStartupTask` 接口创建启动任务：

```csharp
public class DatabaseInitializer : IStartupTask
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("开始初始化数据库");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        _logger.LogInformation("数据库初始化完成");
    }
}
```

### 2. 任务配置

在 `Program.cs` 中配置启动任务：

```csharp
builder.Services.AddStartupTasks(Assembly.GetExecutingAssembly());
```

## 使用方式

### 1. 基本使用

```csharp
public class CacheWarmupTask : IStartupTask
{
    private readonly ICacheService _cacheService;

    public CacheWarmupTask(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task ExecuteAsync()
    {
        await _cacheService.WarmupAsync();
    }
}
```

### 2. 任务顺序

可以使用 `[StartupTaskOrder]` 特性控制任务执行顺序：

```csharp
[StartupTaskOrder(1)]
public class FirstTask : IStartupTask
{
    public async Task ExecuteAsync()
    {
        // 第一个执行的任务
    }
}

[StartupTaskOrder(2)]
public class SecondTask : IStartupTask
{
    public async Task ExecuteAsync()
    {
        // 第二个执行的任务
    }
}
```

## 高级特性

### 1. 任务依赖

可以配置任务之间的依赖关系：

```csharp
[DependsOn(typeof(DatabaseInitializer))]
public class CacheWarmupTask : IStartupTask
{
    public async Task ExecuteAsync()
    {
        // 在数据库初始化完成后执行
    }
}
```

### 2. 任务状态

可以获取和监控任务状态：

```csharp
public class StartupTaskMonitor
{
    private readonly IStartupTaskManager _taskManager;

    public async Task<bool> IsTaskCompleted(string taskName)
    {
        return await _taskManager.IsTaskCompletedAsync(taskName);
    }
}
```

## 最佳实践

1. **任务设计**
   - 保持任务职责单一
   - 避免长时间运行的任务
   - 使用异步方法

2. **错误处理**
   - 实现适当的错误处理
   - 记录任务执行日志
   - 配置重试策略

3. **性能考虑**
   - 避免资源密集型任务
   - 考虑并行执行独立任务
   - 监控任务执行时间

## 示例

### 1. 配置初始化示例

```csharp
public class ConfigurationInitializer : IStartupTask
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationInitializer> _logger;

    public ConfigurationInitializer(IConfiguration configuration, ILogger<ConfigurationInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("开始初始化配置");
            await ValidateConfigurationAsync();
            await LoadDefaultSettingsAsync();
            _logger.LogInformation("配置初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "配置初始化失败");
            throw;
        }
    }
}
```

### 2. 缓存预热示例

```csharp
[StartupTaskOrder(2)]
[DependsOn(typeof(ConfigurationInitializer))]
public class CacheWarmupTask : IStartupTask
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheWarmupTask> _logger;

    public CacheWarmupTask(ICacheService cacheService, ILogger<CacheWarmupTask> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("开始预热缓存");
        await _cacheService.WarmupAsync();
        _logger.LogInformation("缓存预热完成");
    }
}
```

## 下一步

- [发布订阅](EventPublishing.md)
- [作业调度](Scheduling.md)
- [审计](Auditing.md) 