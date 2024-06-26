using Biwen.QuickApi.Events;
using Biwen.QuickApi.Scheduling.Events;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Biwen.QuickApi.Telemetry.Implementation
{
    /// <summary>
    /// 调度仪表盘
    /// </summary>
    internal class SchedulingInstrumentation :
        IEventSubscriber<TaskFailedEvent>,
        IEventSubscriber<TaskSuccessedEvent>,
        IEventSubscriber<TaskStartedEvent>
    {
        /// <summary>
        /// Scheduling ActivitySource Name
        /// </summary>
        public static readonly string SourceName = $"{Constant.OpenTelemetryActivitySourceName}.Scheduling";

        static readonly Meter meter = new Meter(SourceName, Constant.OpenTelemetryVersion);
        /// <summary>
        /// ActivitySource
        /// </summary>
        static readonly ActivitySource SchedulingActivitySource = new ActivitySource(SourceName);

        public Task HandleAsync(TaskSuccessedEvent @event, CancellationToken ct)
        {
            using (var activity = SchedulingActivitySource.StartActivity("TaskSuccessedEvent")!)
            {
                activity.AddTag("TaskId", @event.ScheduleTask.GetType().FullName);
                activity.AddTag("EventTime", @event.EventTime);

                meter.CreateCounter<long>($"{Constant.Prefix}TaskSuccessedCount", "counter", description: "任务成功次数").Add(1);
            }
            return Task.CompletedTask;

        }

        public Task HandleAsync(TaskFailedEvent @event, CancellationToken ct)
        {
            using (var activity = SchedulingActivitySource.StartActivity("TaskFailedEvent")!)
            {
                activity.AddTag("TaskId", @event.ScheduleTask.GetType().FullName);
                activity.AddTag("EventTime", @event.EventTime);
                activity.AddTag("Exception", @event.Exception);

                meter.CreateCounter<long>($"{Constant.Prefix}TaskFailedCount", "counter", description: "任务失败次数").Add(1);
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(TaskStartedEvent @event, CancellationToken ct)
        {
            using (var activity = SchedulingActivitySource.StartActivity("TaskStartedEvent")!)
            {
                activity.AddTag("TaskId", @event.ScheduleTask.GetType().FullName);
                activity.AddTag("EventTime", @event.EventTime);

                meter.CreateCounter<long>($"{Constant.Prefix}TaskStartedCount", "counter", description: "任务启动次数").Add(1);
            }
            return Task.CompletedTask;
        }
    }
}