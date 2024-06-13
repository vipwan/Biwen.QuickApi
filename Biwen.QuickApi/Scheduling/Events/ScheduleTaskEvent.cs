using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.Scheduling.Events
{

    public abstract class ScheduleTaskEvent(IScheduleTask scheduleTask, DateTime eventTime) : IEvent
    {
        /// <summary>
        /// 任务
        /// </summary>
        public IScheduleTask ScheduleTask { get; set; } = scheduleTask;
        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime EventTime { get; set; } = eventTime;
    }

    /// <summary>
    /// 执行完成
    /// </summary>
    /// <param name="scheduleTask"></param>
    /// <param name="endTime">开始时间内</param>
    /// <param name="eventTime">结束时间</param>
    public sealed class TaskSuccessedEvent(IScheduleTask scheduleTask, DateTime eventTime, DateTime endTime) : ScheduleTaskEvent(scheduleTask, eventTime)
    {
        /// <summary>
        /// 执行结束的时间
        /// </summary>
        public DateTime EndTime { get; set; } = endTime;
    }

    /// <summary>
    /// 执行开始
    /// </summary>
    /// <param name="scheduleTask"></param>
    /// <param name="eventTime"></param>
    public sealed class TaskStartedEvent(IScheduleTask scheduleTask, DateTime eventTime) : ScheduleTaskEvent(scheduleTask, eventTime);

    /// <summary>
    /// 执行失败
    /// </summary>
    /// <param name="scheduleTask"></param>
    /// <param name="eventTime"></param>
    /// <param name="exception"></param>
    public sealed class TaskFailedEvent(IScheduleTask scheduleTask, DateTime eventTime, Exception exception) : ScheduleTaskEvent(scheduleTask, eventTime)
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; private set; } = exception;
    }
}