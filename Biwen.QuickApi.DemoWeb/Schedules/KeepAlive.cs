using Biwen.QuickApi.Scheduling;

namespace Biwen.QuickApi.DemoWeb.Schedules
{
    /// <summary>
    /// KeepAlive ScheduleTask
    /// </summary>
    /// <param name="logger"></param>
    [ScheduleTask(Constants.CronEveryMinute)] //每分钟一次
    [ScheduleTask("0/3 * * * *")]//每3分钟执行一次
    public class KeepAlive(ILogger<KeepAlive> logger) : IScheduleTask
    {
        public async Task ExecuteAsync()
        {
            logger.LogInformation("keep alive!");
            await Task.CompletedTask;
        }
    }
}