// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan

namespace Biwen.QuickApi.Events;


/// <summary>
/// 消息发布器
/// </summary>
public interface IPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IEvent;
}

internal class Publisher : IPublisher
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<Publisher> _logger;

    /// <summary>
    /// 缓存订阅者类型的Metadata
    /// </summary>
    private static readonly ConcurrentDictionary<Type, List<(Type SubscriberType, EventSubscriberAttribute Metadata)>> _subscriberTypeMetadatas = new();

    public Publisher(IServiceScopeFactory serviceScopeFactory, ILogger<Publisher> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
    {
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var subscribers = scope.ServiceProvider.GetServices<IEventSubscriber<T>>().ToList();

        // 无订阅者直接返回
        if (subscribers is null || subscribers.Count == 0)
        {
            _logger.LogDebug("No subscribers found for event {EventType}", typeof(T).Name);
            return;
        }

        // 获取或创建订阅者类型元数据缓存
        var subscriberMetadata = _subscriberTypeMetadatas.GetOrAdd(typeof(T), _ =>
        {
            var metadata = new List<(Type SubscriberType, EventSubscriberAttribute Metadata)>();
            foreach (var subscriber in subscribers)
            {
                var type = subscriber.GetType();
                var attribute = type.GetCustomAttribute<EventSubscriberAttribute>() ?? new EventSubscriberAttribute();
                metadata.Add((type, attribute));
            }
            return metadata;
        });

        // 根据订阅者类型元数据创建处理任务列表
        var tasks = new List<Task>();
        foreach (var subscriber in subscribers)
        {
            var subscriberType = subscriber.GetType();
            var metadata = subscriberMetadata.FirstOrDefault(m => m.SubscriberType == subscriberType).Metadata;

            try
            {
                if (metadata.IsAsync)
                {
                    // 异步执行（非阻塞）
                    var fireAndForgetTask = Task.Run(async () =>
                    {
                        try
                        {
                            await subscriber.HandleAsync(@event, ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in async event handler {SubscriberType} for {EventType}",
                                subscriberType.Name, typeof(T).Name);
                        }
                    }, CancellationToken.None);
                }
                else
                {
                    // 同步执行（阻塞）
                    tasks.Add(subscriber.HandleAsync(@event, ct));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event handler {SubscriberType} for {EventType}",
                    subscriberType.Name, typeof(T).Name);

                if (metadata.ThrowIfError)
                {
                    throw;
                }
            }
        }

        // 等待所有非异步任务完成
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }
}
