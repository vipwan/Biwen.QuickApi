using NCrontab;
using System.Collections.Concurrent;
namespace Biwen.QuickApi.Scheduling
{
    internal class BackgroundTaskScheduler
    {
        /// <summary>
        /// 暂存上次执行时间
        /// </summary>
        private static ConcurrentDictionary<ScheduleTaskAttribute, DateTime> LastRunTimes = new();

        public BackgroundTaskScheduler(ScheduleTaskAttribute scheduleMetada, DateTime referenceTime)
        {
            ReferenceTime = referenceTime;
            ScheduleMetada = scheduleMetada;
        }

        public ScheduleTaskAttribute ScheduleMetada { get; }
        public DateTime ReferenceTime { get; set; }

        public bool CanRun()
        {
            var now = DateTime.Now;
            var referenceTime = ReferenceTime;
            var haveExcuteTime = LastRunTimes.TryGetValue(ScheduleMetada, out var time);
            if (!haveExcuteTime)
            {
                var nextStartTime = CrontabSchedule.Parse(ScheduleMetada.Cron).GetNextOccurrence(referenceTime);
                LastRunTimes.TryAdd(ScheduleMetada, nextStartTime);

                //如果不是初始化启动,则不执行
                if (!ScheduleMetada.IsStartOnInit)
                    return false;
            }
            if (now >= time)
            {
                ReferenceTime = DateTime.Now;

                var nextStartTime = CrontabSchedule.Parse(ScheduleMetada.Cron).GetNextOccurrence(referenceTime);
                //更新下次执行时间
                LastRunTimes.TryUpdate(ScheduleMetada, nextStartTime, time);
                return true;
            }
            return false;
        }
    }
}
