分布式锁
=====================
集成`DistributedLock.Redis`的分布式锁

参考代码
---------------------
提供分布式锁,参考代码:
```csharp
//注入服务
services.AddLocalLock();
```
单元测试
---------------------
```csharp
[Fact]
public async Task TestDistributedLock()
{
    var services = new ServiceCollection();

    services.AddDistributedLock();//localhost:6379

    var provider = services.BuildServiceProvider();
    var lockProvider = provider.GetRequiredService<IDistributedLockProvider>();

    var key = "test-1";

    _ = Task.Run(async () =>
    {
        using var distributedLock = await lockProvider.TryAcquireLockAsync(key, TimeSpan.FromSeconds(10));
        // 模拟一个长时间的任务
        await Task.Delay(TimeSpan.FromSeconds(3));
    });

    await Task.Delay(500);

    //当前线程尝试获取锁, 由于上一个线程持有锁, 所以获取锁失败
    using var distributedLock2 = await lockProvider.TryAcquireLockAsync(key, TimeSpan.FromSeconds(1));
    Assert.Null(distributedLock2);

    await Task.Delay(3 * 1000);
    //上一个线程释放锁后, 当前线程获取锁成功
    using var distributedLock3 = await lockProvider.TryAcquireLockAsync(key, TimeSpan.FromSeconds(1));
    Assert.NotNull(distributedLock3);
}

```

API文档
---------------------

相关API文档:
