﻿// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 Publisher.cs

namespace Biwen.QuickApi.Events;

internal class Publisher(IServiceScopeFactory serviceScopeFactory)
{
    /// <summary>
    /// 缓存订阅者的Metadata
    /// </summary>
    static readonly ConcurrentDictionary<Type, object> SubscriberMetadatas = new();

    public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
    {
        using var scope = serviceScopeFactory.CreateAsyncScope();
        var subscribers = scope.ServiceProvider.GetServices<IEventSubscriber<T>>();
        //无订阅者直接返回
        if (subscribers is null || !subscribers.Any()) return;

        if (SubscriberMetadatas.GetOrAdd(typeof(T), type =>
          {
              List<(IEventSubscriber<T> Subscriber, EventSubscriberAttribute Metadata)> metas = [];
              foreach (var subscriber in subscribers)
              {
                  var metadata = subscriber.GetType().GetCustomAttribute<EventSubscriberAttribute>() ?? new();
                  metas.Add((subscriber, metadata));
              }
              return metas;
          }) is not List<(IEventSubscriber<T> Subscriber, EventSubscriberAttribute Metadata)> listWithMetadatas) return;

        foreach (var (subscriber, metadata) in listWithMetadatas.OrderBy(x => x.Metadata.Order))
        {
            try
            {
                if (!metadata.IsAsync)
                {
                    await subscriber.HandleAsync(@event, ct);
                }
                else
                {
                    _ = subscriber.HandleAsync(@event, ct);
                }
            }
            catch
            {
                if (metadata.ThrowIfError)
                {
                    throw;
                }
                //todo:
            }
        }
    }
}