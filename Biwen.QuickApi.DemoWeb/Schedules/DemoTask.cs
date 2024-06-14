namespace Biwen.QuickApi.DemoWeb.Schedules
{
    using Biwen.QuickApi.Infrastructure.Locking;
    using Biwen.QuickApi.Scheduling.Stores;

    /// <summary>
    /// Demo ScheduleTask，用于Store演示
    /// </summary>
    /// <param name="logger"></param>
    public class DemoTask(ILogger<DemoTask> logger, ILocalLock @lock) : IScheduleTask
    {
        public async Task ExecuteAsync()
        {
            var timeout = TimeSpan.FromMilliseconds(30_000);

            //使用ILocalLock进行锁定,防止重复执行
            var (locker, _) = await @lock.TryAcquireLockAsync("ONLY_ONE_ScheduleTask_OF_DemoTask", timeout, timeout);

            if (locker is null)
            {
                return;//当前锁定,则不执行!
            }

            using (locker)
            {
                await Task.Delay(31_000);
                logger.LogInformation("Demo memory store Schedule Done!");
            }
        }
    }

    /// <summary>
    /// 配置store的 ScheduleTask，用于Store演示
    /// </summary>
    /// <param name="logger"></param>
    public class DemoConfigTask(ILogger<DemoConfigTask> logger) : IScheduleTask
    {
        public Task ExecuteAsync()
        {
            logger.LogInformation("Demo Config store Schedule Done!");
            return Task.CompletedTask;
        }
    }


    public class DemoStore : IScheduleMetadataStore
    {
        public Task<ScheduleTaskMetadata[]> GetAllAsync()
        {
            //模拟从数据库或配置文件中获取ScheduleTaskMetadata
            ScheduleTaskMetadata[] metadatas =
                [
                    new ScheduleTaskMetadata(typeof(DemoTask),Constants.CronEveryNMinutes(2))
                    {
                        IsStartOnInit =true,
                        Description="测试的Schedule"
                    },
                ];

            return Task.FromResult(metadatas);
        }
    }
}