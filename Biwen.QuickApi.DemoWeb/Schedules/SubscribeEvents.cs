using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.DemoWeb.Schedules
{
    using Biwen.QuickApi.Scheduling.Events;
    using System.Threading;
    using System.Threading.Tasks;

    [EventSubscriber(IsAsync = true)]
    public class WhenDoneEvent(ILogger<WhenDoneEvent> logger) : IEventSubscriber<ScheduleTaskSuccessed>
    {
        public Task HandleAsync(ScheduleTaskSuccessed @event, CancellationToken ct)
        {
            if (@event.ScheduleTask is KeepAlive)
            {
                logger.LogInformation($".KeepAlive Done!..............");
            }
            return Task.CompletedTask;
        }
    }
}
