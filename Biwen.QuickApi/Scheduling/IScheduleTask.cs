namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// 任务调度接口
    /// </summary>
    public interface IScheduleTask
    {
        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
    }

    /// <summary>
    /// ScheduleTask 抽象类
    /// </summary>
    public abstract class ScheduleTask : IScheduleTask
    {
        public virtual Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 同一时间只能存在一个的任务
    /// </summary>
    public abstract class OnlyOneRunningScheduleTask : ScheduleTask
    {
        /// <summary>
        /// 如果有正在运行的相同任务,打断当前的执行的回调
        /// </summary>
        /// <returns></returns>
        public virtual Task OnAbort()
        {
            return Task.CompletedTask;
        }
    }
}