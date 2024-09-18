// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:52:26 SuccessedEventHandler.cs

using Biwen.QuickApi.Events;

namespace Biwen.QuickApi.Scheduling.Events;

/// <summary>
/// 订阅Schedule完成通知
/// </summary>
/// <param name="logger"></param>
[EventSubscriber(IsAsync = true)]
public class SuccessedEventHandler(ILogger<SuccessedEventHandler> logger) : IEventSubscriber<TaskSuccessedEvent>
{
    public virtual Task HandleAsync(TaskSuccessedEvent @event, CancellationToken ct)
    {
#if DEBUG
        logger.LogDebug($"[{@event.EventTime}] ScheduleTask:{@event.ScheduleTask.GetType().FullName} Successed!");
#endif
        return Task.CompletedTask;
    }
}