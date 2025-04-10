# 本地锁

Biwen.QuickApi 提供了本地锁功能，用于在单机环境下实现资源同步。

## 基本概念

### 1. 锁定义

使用 `ILocalLock` 接口实现本地锁：

```csharp
public class ResourceService
{
    private readonly ILocalLock _localLock;

    public ResourceService(ILocalLock localLock)
    {
        _localLock = localLock;
    }

    public async Task UpdateResource(string resourceId)
    {
        using var @lock = await _localLock.AcquireAsync(resourceId);
        if (@lock.IsAcquired)
        {
            // 执行需要同步的操作
            await DoSomethingAsync();
        }
    }
}
```

### 2. 锁配置

在 `Program.cs` 中配置本地锁：

```csharp
builder.Services.AddLocalLock();
```

## 使用方式

### 1. 基本使用

```csharp
public class ResourceApi : BaseQuickApi
{
    private readonly ILocalLock _localLock;

    public ResourceApi(ILocalLock localLock)
    {
        _localLock = localLock;
    }

    public async Task<Result> Update(string resourceId)
    {
        using var @lock = await _localLock.AcquireAsync(resourceId, TimeSpan.FromSeconds(30));
        if (!@lock.IsAcquired)
        {
            return Result.Fail("无法获取资源锁");
        }

        // 执行更新操作
        await UpdateResourceAsync(resourceId);
        return Result.Success();
    }
}
```

### 2. 超时配置

可以配置锁的获取超时时间：

```csharp
using var @lock = await _localLock.AcquireAsync(
    resourceId,
    TimeSpan.FromSeconds(10), // 等待时间
    TimeSpan.FromSeconds(30)  // 锁的持有时间
);
```

## 高级特性

### 1. 锁的自动释放

使用 `using` 语句确保锁的自动释放：

```csharp
public async Task DoSomethingAsync()
{
    using var @lock = await _localLock.AcquireAsync("resource");
    if (@lock.IsAcquired)
    {
        // 执行操作
    }
    // 锁会在作用域结束时自动释放
}
```

### 2. 锁的监控

可以监控锁的状态：

```csharp
public class LockMonitor
{
    private readonly ILocalLock _localLock;

    public async Task<bool> IsLocked(string resourceId)
    {
        return await _localLock.IsLockedAsync(resourceId);
    }
}
```

## 最佳实践

1. **锁的范围**
   - 尽量缩小锁的范围
   - 避免在锁内执行耗时操作
   - 使用适当的超时时间

2. **错误处理**
   - 处理锁获取失败的情况
   - 确保锁的释放
   - 记录锁的使用情况

3. **性能考虑**
   - 避免过度使用锁
   - 考虑使用其他同步机制
   - 监控锁的竞争情况

## 示例

### 1. 资源更新示例

```csharp
public class ResourceUpdater
{
    private readonly ILocalLock _localLock;
    private readonly ILogger<ResourceUpdater> _logger;

    public ResourceUpdater(ILocalLock localLock, ILogger<ResourceUpdater> logger)
    {
        _localLock = localLock;
        _logger = logger;
    }

    public async Task UpdateResourceAsync(string resourceId)
    {
        try
        {
            using var @lock = await _localLock.AcquireAsync(resourceId);
            if (!@lock.IsAcquired)
            {
                _logger.LogWarning("无法获取资源 {ResourceId} 的锁", resourceId);
                return;
            }

            _logger.LogInformation("开始更新资源 {ResourceId}", resourceId);
            await DoUpdateAsync(resourceId);
            _logger.LogInformation("资源 {ResourceId} 更新完成", resourceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新资源 {ResourceId} 时发生错误", resourceId);
            throw;
        }
    }
}
```

### 2. 批量操作示例

```csharp
public class BatchProcessor
{
    private readonly ILocalLock _localLock;

    public async Task ProcessBatchAsync(string[] resourceIds)
    {
        foreach (var resourceId in resourceIds)
        {
            using var @lock = await _localLock.AcquireAsync(resourceId);
            if (@lock.IsAcquired)
            {
                await ProcessResourceAsync(resourceId);
            }
        }
    }
}
```

## 下一步

- [分布式锁](DistributedLock.md)
- [UnitOfWork](UnitOfWork.md)
- [审计](Auditing.md) 