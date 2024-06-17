﻿namespace Biwen.QuickApi.Events
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
        public static async Task PublishAsync<T>(this T @event, CancellationToken cancellationToken = default) where T : class, IEvent
        {
            if (ServiceRegistration.ServiceProvider is null) throw new QuickApiExcetion("mush UseBiwenQuickApis() first!");
            var publisher = ActivatorUtilities.GetServiceOrCreateInstance<Publisher>(ServiceRegistration.ServiceProvider);
            await publisher.PublishAsync(@event, cancellationToken);
        }
    }
}