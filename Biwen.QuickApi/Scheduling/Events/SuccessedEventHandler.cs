using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.Scheduling.Events
{
    /// <summary>
    /// 订阅Schedule完成通知
    /// </summary>
    /// <param name="logger"></param>
    [EventSubscriber(IsAsync = true)]
    public class SuccessedEventHandler(ILogger<SuccessedEventHandler> logger) : IEventSubscriber<TaskSuccessedEvent>
    {
        public virtual Task HandleAsync(TaskSuccessedEvent @event, CancellationToken ct)
        {
            logger.LogDebug($"[{@event.EventTime}] ScheduleTask:{@event.ScheduleTask.GetType().FullName} Successed!");
            return Task.CompletedTask;
        }
    }
}