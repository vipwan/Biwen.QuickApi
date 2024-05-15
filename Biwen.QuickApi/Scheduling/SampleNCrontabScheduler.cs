using NCrontab;
using System.Collections.Concurrent;
namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// BackgroundTaskScheduler 后台任务调度器,用于判断任务是否可以执行
    /// </summary>
    internal class SampleNCrontabScheduler : IScheduler
    {
        /// <summary>
        /// 暂存上次执行时间
        /// </summary>
        private static ConcurrentDictionary<ScheduleTaskAttribute, DateTime> LastRunTimes = new();

        public bool CanRun(ScheduleTaskAttribute scheduleMetadata, DateTime referenceTime)
        {
            var now = DateTime.Now;
            var haveExcuteTime = LastRunTimes.TryGetValue(scheduleMetadata, out var time);
            if (!haveExcuteTime)
            {
                var nextStartTime = CrontabSchedule.Parse(scheduleMetadata.Cron).GetNextOccurrence(referenceTime);
                LastRunTimes.TryAdd(scheduleMetadata, nextStartTime);

                //如果不是初始化启动,则不执行
                if (!scheduleMetadata.IsStartOnInit)
                    return false;
            }
            if (now >= time)
            {
                var nextStartTime = CrontabSchedule.Parse(scheduleMetadata.Cron).GetNextOccurrence(referenceTime);
                //更新下次执行时间
                LastRunTimes.TryUpdate(scheduleMetadata, nextStartTime, time);
                return true;
            }
            return false;
        }
    }
}
