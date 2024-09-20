// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:35 LocalLockTest.cs

using Biwen.QuickApi.Infrastructure.Locking;
using Microsoft.Extensions.Logging;

namespace Biwen.QuickApi.Test;

public class LocalLockTest
{
    [Fact]
    public async Task AcquireTest()
    {
        using var loggerFactory = LoggerFactory.Create(cfg =>
        {
            cfg.AddConsole();
        });
        var logger = loggerFactory.CreateLogger<LocalLock>();

        var localLock = new LocalLock(logger);

        var first = () =>
        {
            return Task.Run(async () =>
            {
                var (locker, locked) = await localLock.TryAcquireLockAsync("TEST_KEY", TimeSpan.FromSeconds(5));
                //首次获取锁,应该获取到
                Assert.True(locker is not null);
                if (locker is not null)
                {
                    using (locker)
                    {
                        await Task.Delay(10_000);
                    }
                }
            });
        };

        var second = () =>
        {
            return Task.Run(async () =>
            {
                var (locker, locked) = await localLock.TryAcquireLockAsync("TEST_KEY", TimeSpan.FromSeconds(5));
                //第二次获取锁,应该获取不到,因为第一个任务还在执行
                Assert.True(locker is null);
            });
        };

        var third = () =>
        {
            return Task.Run(async () =>
            {
                await Task.Delay(15_000);
                var (locker, locked) = await localLock.TryAcquireLockAsync("TEST_KEY", TimeSpan.FromSeconds(5));
                //第三次获取锁,应该获取到,因为第一个任务已经执行完毕
                Assert.True(locker is not null);
            });
        };

        //启动锁
        _ = Task.Run(first);

        await Task.Delay(1000);

        //测试断言
        await Task.WhenAll(second(), third());

    }
}