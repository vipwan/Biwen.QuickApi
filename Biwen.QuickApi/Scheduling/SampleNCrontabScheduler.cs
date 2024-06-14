using NCrontab;
using System.Collections.Concurrent;
namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// 使用NCrontab实现的调度器,不支持秒级,这是默认调度器
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
