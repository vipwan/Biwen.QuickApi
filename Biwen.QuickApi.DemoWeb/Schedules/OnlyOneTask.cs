namespace Biwen.QuickApi.DemoWeb.Schedules
{
    /// <summary>
    /// 模拟一个只能同时存在一个的任务.一分钟执行一次,但是耗时两分钟.
    /// </summary>
    /// <param name="logger"></param>
    [ScheduleTask(Constants.CronEveryMinute, IsStartOnInit = true)]
    public class OnlyOneTask(ILogger<OnlyOneTask> logger) : OnlyOneRunningScheduleTask
    {
        public override Task OnAbort()
        {
            logger.LogWarning($"[{DateTime.Now}]任务被打断.因为有一个相同的任务正在执行!");
            return Task.CompletedTask;
        }

        public override async Task ExecuteAsync()
        {
            var now = DateTime.Now;
            //模拟一个耗时2分钟的任务
            await Task.Delay(TimeSpan.FromMinutes(2));
            logger.LogInformation($"[{now}] ~ {DateTime.Now} 执行一个耗时两分钟的任务!");
        }
    }
}