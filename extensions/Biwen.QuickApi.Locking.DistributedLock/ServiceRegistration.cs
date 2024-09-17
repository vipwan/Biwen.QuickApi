// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-17 16:59:55 ServiceRegistration.cs

using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Biwen.QuickApi.Infrastructure.Locking.DistributedLock;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 添加Redis分布式锁服务
    /// 使用方式:注入<see cref="IDistributedLockProvider"/>,使用<seealso cref="DistributedLockProviderExtensions.AcquireLockAsync(IDistributedLockProvider, string, TimeSpan?, CancellationToken)"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="redisConnStr"></param>
    public static void AddDistributedLock(this IServiceCollection services, string redisConnStr = "localhost:6379")
    {
        ArgumentNullException.ThrowIfNullOrEmpty(redisConnStr);

        // 配置 Redis 连接
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnStr, true);
            return ConnectionMultiplexer.Connect(configuration);
        });

        // 配置 DistributedLock
        services.TryAddSingleton<IDistributedLockProvider>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisDistributedSynchronizationProvider(multiplexer.GetDatabase());
        });

    }
}
