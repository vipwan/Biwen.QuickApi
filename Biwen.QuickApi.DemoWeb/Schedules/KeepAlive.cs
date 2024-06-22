namespace Biwen.QuickApi.DemoWeb.Schedules
{
    /// <summary>
    /// KeepAlive ScheduleTask
    /// </summary>
    /// <param name="logger"></param>
    [ScheduleTask("0/3 * * * *")]//每3分钟执行一次
    public class KeepAlive(ILogger<KeepAlive> logger) : IScheduleTask
    {
        public async Task ExecuteAsync()
        {
            //执行5s
            await Task.Delay(TimeSpan.FromSeconds(5));
            logger.LogInformation("keep alive!");
        }
    }

    /// <summary>
    /// 模拟15秒一次的任务
    /// </summary>
    /// <param name="logger"></param>
    [ScheduleTask("15:SECONDS")]//每隔15秒的任务
    public class Every15SecondTask(ILogger<Every15SecondTask> logger) : IScheduleTask
    {
        public async Task ExecuteAsync()
        {
            //执行5s
            await Task.Delay(TimeSpan.FromSeconds(5));
            logger.LogInformation("Every 15 Seconds !");
        }
    }

}