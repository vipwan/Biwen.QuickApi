// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:53:13 IScheduler.cs

namespace Biwen.QuickApi.Scheduling;

/// <summary>
/// 调度器
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// 判断当前的任务是否可以执行
    /// </summary>
    /// <param name="scheduleMetadata"></param>
    /// <param name="referenceTime"></param>
    /// <returns></returns>
    bool CanRun(ScheduleTaskAttribute scheduleMetadata, DateTime referenceTime);
}