using NCrontab;
using System.Collections.Concurrent;
namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// BackgroundTaskScheduler 后台任务调度器,用于判断任务是否可以执行
    /// </summary>
    internal class BackgroundTaskScheduler
    {
        /// <summary>
        /// 暂存上次执行时间
        /// </summary>
        private static ConcurrentDictionary<ScheduleTaskAttribute, DateTime> LastRunTimes = new();

        public BackgroundTaskScheduler(ScheduleTaskAttribute scheduleMetadata, DateTime referenceTime)
        {
            ReferenceTime = referenceTime;
            ScheduleMetadata = scheduleMetadata;
        }

        public ScheduleTaskAttribute ScheduleMetadata { get; }
        public DateTime ReferenceTime { get; set; }

        public bool CanRun()
        {
            var now = DateTime.Now;
            var referenceTime = ReferenceTime;
            var haveExcuteTime = LastRunTimes.TryGetValue(ScheduleMetadata, out var time);
            if (!haveExcuteTime)
            {
                var nextStartTime = CrontabSchedule.Parse(ScheduleMetadata.Cron).GetNextOccurrence(referenceTime);
                LastRunTimes.TryAdd(ScheduleMetadata, nextStartTime);

                //如果不是初始化启动,则不执行
                if (!ScheduleMetadata.IsStartOnInit)
                    return false;
            }
            if (now >= time)
            {
                ReferenceTime = DateTime.Now;

                var nextStartTime = CrontabSchedule.Parse(ScheduleMetadata.Cron).GetNextOccurrence(referenceTime);
                //更新下次执行时间
                LastRunTimes.TryUpdate(ScheduleMetadata, nextStartTime, time);
                return true;
            }
            return false;
        }
    }
}
