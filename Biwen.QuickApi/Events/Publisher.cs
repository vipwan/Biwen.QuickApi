namespace Biwen.QuickApi.Events
{
    internal class Publisher
    {
        private readonly IServiceProvider _serviceProvider;

        public Publisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<T>(T @event) where T : IEvent
        {
            var handlers = _serviceProvider.GetServices<IEventHandler<T>>();
            //var handlers = (IEnumerable<IEventHandler<T>>)(_serviceProvider.GetService(typeof(IEnumerable<IEventHandler<T>>)) ?? throw new NotSupportedException());
            if (handlers is null) return;

            foreach (var handler in handlers.OrderBy(x => x.Order))
            {
                try
                {
                    await handler.HandleAsync(@event, default);
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