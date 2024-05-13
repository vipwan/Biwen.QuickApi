namespace Biwen.QuickApi.Events
{
    public static class EventExtensions
    {
        /// <summary>
        /// 事件广播扩展
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task PublishAsync<T>(this T @event, CancellationToken cancellationToken = default) where T : IEvent
        {
            if (ServiceRegistration.ServiceProvider is null) throw new QuickApiExcetion("mush UseBiwenQuickApis() first!");
            using var scope = ServiceRegistration.ServiceProvider.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<Publisher>();
            await publisher.PublishAsync(@event, cancellationToken);
        }
    }
}