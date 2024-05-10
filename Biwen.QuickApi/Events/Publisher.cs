namespace Biwen.QuickApi.Events
{
    internal class Publisher(IServiceProvider serviceProvider)
    {
        public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
        {
            var handlers = serviceProvider.GetServices<IEventSubscriber<T>>();
            if (handlers is null) return;
            foreach (var handler in handlers.OrderBy(x => x.Order))
            {
                try
                {
                    await handler.HandleAsync(@event, ct);
                }
                catch
                {
                    if (handler.ThrowIfError)
                    {
                        throw;
                    }
                    //todo:
                }
            }
        }
    }
}