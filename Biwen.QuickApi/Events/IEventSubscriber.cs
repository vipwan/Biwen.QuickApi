// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IEventSubscriber.cs

namespace Biwen.QuickApi.Events;

/// <summary>
/// 事件订阅者,如果需要排序&异步或者抛出异常请给订阅者标注特性<see cref="EventSubscriberAttribute"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventSubscriber<T> where T : IEvent
{
    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="event">事件</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task HandleAsync(T @event, CancellationToken ct);
}

/// <inheritdoc cref = "IEventSubscriber{T}"/>
public abstract class EventSubscriber<T> : IEventSubscriber<T> where T : IEvent
{
    public abstract Task HandleAsync(T @event, CancellationToken ct);
}