namespace Biwen.QuickApi.Events
{
    internal class Publisher(IServiceProvider serviceProvider)
    {
        public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
        {
            var subscribers = serviceProvider.GetServices<IEventSubscriber<T>>();
            //无订阅者直接返回
            if (subscribers is null) return;
            if (subscribers.Any() == false) return;

            List<(IEventSubscriber<T> Subscriber, EventSubscriberAttribute Metadata)> listWithMetadatas = [];

            foreach (var subscriber in subscribers)
            {
                var metadata = subscriber.GetType().GetCustomAttribute<EventSubscriberAttribute>() ?? new EventSubscriberAttribute();
                listWithMetadatas.Add((subscriber, metadata));
            }

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