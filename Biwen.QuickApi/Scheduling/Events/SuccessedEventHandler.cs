﻿using Biwen.QuickApi.Events;
using Biwen.QuickApi.Scheduling.Events;

namespace Biwen.QuickApi.Scheduling
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
            logger.LogInformation($"[{@event.EventTime}] ScheduleTask:{@event.ScheduleTask.GetType().FullName} Successed!");
            return Task.CompletedTask;
        }
    }
}