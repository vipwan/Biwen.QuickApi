namespace Biwen.QuickApi.Scheduling
{
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
}