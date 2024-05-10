namespace Biwen.QuickApi.Events
{
    internal class Publisher(IServiceProvider serviceProvider)
    {
        public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
        {
            var subscribers = serviceProvider.GetServices<IEventSubscriber<T>>();
            if (subscribers is null) return;

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