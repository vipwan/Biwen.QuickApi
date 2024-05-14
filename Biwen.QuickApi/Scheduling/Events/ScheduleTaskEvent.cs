using Biwen.QuickApi.Events;
using System;

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
    /// <typeparam name="T"></typeparam>
    /// <param name="scheduleTask"></param>
    /// <param name="eventTime"></param>
    public sealed class ScheduleTaskSuccessed(IScheduleTask scheduleTask, DateTime eventTime) : ScheduleTaskEvent(scheduleTask, eventTime);

    /// <summary>
    /// 执行开始
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="scheduleTask"></param>
    /// <param name="eventTime"></param>
    public sealed class ScheduleTaskStarted(IScheduleTask scheduleTask, DateTime eventTime) : ScheduleTaskEvent(scheduleTask, eventTime);

    /// <summary>
    /// 执行失败
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="scheduleTask"></param>
    /// <param name="eventTime"></param>
    public sealed class ScheduleTaskFailed(IScheduleTask scheduleTask, DateTime eventTime, Exception exception) : ScheduleTaskEvent(scheduleTask, eventTime)
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception Exception { get; private set; } = exception;
    }
}