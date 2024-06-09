namespace Biwen.QuickApi.DemoWeb.Schedules
{
    using Biwen.QuickApi.Scheduling.Events;

    [EventSubscriber(IsAsync = true)]
    public class DemoTaskSuccessedEvent(ILogger<DemoTaskSuccessedEvent> logger) : IEventSubscriber<TaskSuccessedEvent>
    {
        public Task HandleAsync(TaskSuccessedEvent @event, CancellationToken ct)
        {
            //只处理Keepalive完成的事件
            if (@event.ScheduleTask is KeepAlive)
            {
                logger.LogInformation($".KeepAlive Done! 执行时间:{@event.EventTime},结束时间:{@event.EndTime}");
            }
            //如果有需要可以持久化或者通知其他服务
            return Task.CompletedTask;
        }
    }
}
