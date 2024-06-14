﻿using System.Collections.Concurrent;

namespace Biwen.QuickApi.Events
{
    internal class Publisher(IServiceProvider serviceProvider)
    {
        /// <summary>
        /// 缓存订阅者的Metadata
        /// </summary>
        private static readonly ConcurrentDictionary<Type, object> SubscriberMetadatas = new();

        public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
        {
            var subscribers = serviceProvider.GetServices<IEventSubscriber<T>>();
            //无订阅者直接返回
            if (subscribers is null) return;
            if (subscribers.Any() == false) return;

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
}