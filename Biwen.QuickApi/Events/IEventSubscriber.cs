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