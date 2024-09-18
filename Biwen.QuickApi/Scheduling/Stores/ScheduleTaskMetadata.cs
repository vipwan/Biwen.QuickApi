// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:53:04 ScheduleTaskMetadata.cs

namespace Biwen.QuickApi.Scheduling.Stores;

/// <summary>
/// ScheduleTask Metadata
/// 请注意如果如果ScheduleTaskType&&Cron&&Description&&IsAsync&&IsStartOnInit都相同，会被认为是同一个任务,所以请确保这些属性的唯一性
/// </summary>
/// <param name="scheduleTaskType"></param>
/// <param name="cron"></param>
public class ScheduleTaskMetadata(Type scheduleTaskType, string cron)
{
    /// <summary>
    /// ScheduleTaskType
    /// </summary>
    public Type ScheduleTaskType { get; set; } = scheduleTaskType;

    /// <summary>
    /// Cron表达式:五位码 <see href="https://en.wikipedia.org/wiki/Cron"/>
    /// </summary>
    public string Cron { get; set; } = cron;

    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否异步执行.默认false会阻塞接下来的同类任务
    /// </summary>
    public bool IsAsync { get; set; } = false;

    /// <summary>
    /// 是否初始化即启动,默认false
    /// </summary>
    public bool IsStartOnInit { get; set; } = false;

}