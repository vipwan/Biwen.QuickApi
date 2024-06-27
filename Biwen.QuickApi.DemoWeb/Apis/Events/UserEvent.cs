namespace Biwen.QuickApi.DemoWeb.Apis.Events
{
    using Biwen.QuickApi.DemoWeb.Db.Entity;
    using Biwen.QuickApi.Service.EntityEvents;
    using System.Threading;
    using System.Threading.Tasks;

    public class UserEvent(ILogger<UserEvent> logger) :
        IEventSubscriber<EntityAdded<User>>,
        IEventSubscriber<EntityDeleted<User>>,
        IEventSubscriber<EntityUpdated<User>>
    {
        public Task HandleAsync(EntityAdded<User> @event, CancellationToken ct)
        {
            logger.LogInformation($"User实体添加,{@event.Entity.Id},{@event.Entity.Name}");
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityDeleted<User> @event, CancellationToken ct)
        {
            logger.LogInformation($"User实体删除,{@event.Entity.Id},{@event.Entity.Name}");
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdated<User> @event, CancellationToken ct)
        {
            logger.LogInformation($"User实体更新,{@event.Entity.Id},{@event.Entity.Name}");
            return Task.CompletedTask;
        }
    }
}