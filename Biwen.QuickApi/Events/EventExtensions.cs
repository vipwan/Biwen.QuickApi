// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 EventExtensions.cs

namespace Biwen.QuickApi.Events;

[SuppressType]
public static class EventExtensions
{
    /// <summary>
    /// 事件广播扩展
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task PublishAsync<T>(this T @event, CancellationToken cancellationToken = default) where T : class, IEvent
    {
#if DEBUG
        //如果是单元测试环境,没有完整的网络环境,则不需要发布事件
        if (QuickApi.ServiceRegistration.ServiceProvider is null)
        {
            return;
        }
#endif
        if (QuickApi.ServiceRegistration.ServiceProvider is null) throw new QuickApiExcetion("must UseBiwenQuickApis() first!");
        var publisher = ActivatorUtilities.GetServiceOrCreateInstance<IPublisher>(QuickApi.ServiceRegistration.ServiceProvider);
        await publisher.PublishAsync(@event, cancellationToken);
    }
}