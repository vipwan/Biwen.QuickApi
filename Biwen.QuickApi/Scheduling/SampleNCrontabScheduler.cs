// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:53:22 SampleNCrontabScheduler.cs

using NCrontab;

namespace Biwen.QuickApi.Scheduling;

/// <summary>
/// 使用NCrontab实现的调度器,支持秒级(10s+),这是默认调度器
/// </summary>
public sealed class SampleNCrontabScheduler : IScheduler
{
    /// <summary>
    /// 暂存上次执行时间
    /// </summary>
    private static ConcurrentDictionary<ScheduleTaskAttribute, DateTime> LastRunTimes = new();

    public bool CanRun(ScheduleTaskAttribute scheduleMetadata, DateTime referenceTime)
    {
        var now = DateTime.Now;
        var haveExcuteTime = LastRunTimes.TryGetValue(scheduleMetadata, out var time);
        var nextStartTime = GetNextStartTime(scheduleMetadata, referenceTime);

        if (!haveExcuteTime)
        {
            LastRunTimes.TryAdd(scheduleMetadata, nextStartTime);

            //如果不是初始化启动,则不执行
            if (!scheduleMetadata.IsStartOnInit)
                return false;
        }

        if (now >= time)
        {
            LastRunTimes.TryUpdate(scheduleMetadata, nextStartTime, time);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取下次执行时间
    /// </summary>
    /// <param name="scheduleMetadata"></param>
    /// <param name="referenceTime"></param>
    /// <returns></returns>
    private DateTime GetNextStartTime(ScheduleTaskAttribute scheduleMetadata, DateTime referenceTime)
    {
        if (IsSecondLevel(scheduleMetadata.Cron, out var seconds))
        {
            return referenceTime.AddSeconds(seconds);
        }
        else
        {
            return CrontabSchedule.Parse(scheduleMetadata.Cron).GetNextOccurrence(referenceTime);
        }
    }
    /// <summary>
    /// 判断是否是秒级
    /// </summary>
    /// <param name="cron"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private bool IsSecondLevel(string cron, out int seconds)
    {
        var cronArray = cron.Split(':');
        if (cronArray.Length == 2 && cronArray[1] == "SECONDS" && int.TryParse(cronArray[0], out seconds))
        {
            return true;
        }
        seconds = 0;
        return false;
    }
}