using Biwen.QuickApi.Scheduling;
using Biwen.QuickApi.Scheduling.Stores;

namespace Biwen.QuickApi.DemoWeb.Schedules
{
    /// <summary>
    /// Demo ScheduleTask，用于Store演示
    /// </summary>
    /// <param name="logger"></param>
    public class DemoTask(ILogger<DemoTask> logger) : IScheduleTask
    {
        public Task ExecuteAsync()
        {
            logger.LogInformation("Demo Schedule Done!");
            return Task.CompletedTask;
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
            logger.LogInformation("Demo Config Schedule Done!");
            return Task.CompletedTask;
        }
    }


    public class DemoStore : IScheduleMetadataStore
    {
        public Task<IEnumerable<ScheduleTaskMetadata>> GetAllAsync()
        {
            //模拟从数据库或配置文件中获取ScheduleTaskMetadata
            IEnumerable<ScheduleTaskMetadata> metadatas =
                [
                    new ScheduleTaskMetadata(typeof(DemoTask),Constants.CronEveryNMinutes(2))
                    {
                        Description="测试的Schedule"
                    },
                ];

            return Task.FromResult(metadatas);
        }
    }
}