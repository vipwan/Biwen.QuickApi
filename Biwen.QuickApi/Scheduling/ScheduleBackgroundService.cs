using Biwen.QuickApi.Scheduling.Stores;
using Microsoft.Extensions.Hosting;
namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// ScheduleBackgroundService
    /// </summary>
    internal class ScheduleBackgroundService : BackgroundService
    {

        private static readonly TimeSpan _pollingTime
#if DEBUG
          //轮询20s 测试环境下，方便测试。
          = TimeSpan.FromSeconds(20);
#endif
#if !DEBUG
         //轮询60s 正式环境下，考虑性能轮询时间延长到60s
         = TimeSpan.FromSeconds(60);
#endif

        //延时10s.
        private static readonly TimeSpan _minIdleTime = TimeSpan.FromSeconds(10);

        private readonly ILogger<ScheduleBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ScheduleBackgroundService(ILogger<ScheduleBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Init the delay first to be also waited on exception.
                var pollingDelay = Task.Delay(_pollingTime, stoppingToken);
                try
                {
                    await RunAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    //todo:
                    _logger.LogError(ex.Message);
                }
                await WaitAsync(pollingDelay, stoppingToken);
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var schedulers = scope.ServiceProvider.GetServices<IScheduleTask>();
            if (schedulers is null || !schedulers.Any())
            {
                return;
            }

            async Task DoTaskAsync(IScheduleTask scheduler, ScheduleTaskAttribute metadata)
            {
                var timeScheduler = new BackgroundTaskScheduler(metadata, DateTime.Now);
                if (timeScheduler.CanRun())
                {
                    if (metadata.IsAsync)
                    {
                        //异步执行
                        _ = scheduler.ExecuteAsync();
                    }
                    else
                    {
                        //同步执行
                        await scheduler.ExecuteAsync();
                    }
                    _logger.LogInformation($"[{DateTime.Now}] ScheduleTask:{scheduler.GetType().FullName} Done!");
                }
            };

            //注解中的scheduler
            foreach (var scheduler in schedulers)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                //标注的metadatas
                var metadatas = scheduler.GetType().GetCustomAttributes<ScheduleTaskAttribute>();

                if (!metadatas.Any())
                {
                    continue;
                }
                foreach (var metadata in metadatas)
                {
                    await DoTaskAsync(scheduler, metadata);
                }
            }

            //store中的scheduler
            var stores = _serviceProvider.GetServices<IScheduleMetadataStore>().ToArray();

            //并行执行,提高性能
            Parallel.ForEach(stores, async store =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                var metadatas = await store.GetAllAsync();
                if (metadatas is null || !metadatas.Any())
                {
                    return;
                }
                foreach (var metadata in metadatas)
                {
                    var attr = new ScheduleTaskAttribute(metadata.Cron)
                    {
                        Description = metadata.Description,
                        IsAsync = metadata.IsAsync,
                        IsStartOnInit = metadata.IsStartOnInit,
                    };

                    var scheduler = scope.ServiceProvider.GetRequiredService(metadata.ScheduleTaskType) as IScheduleTask;
                    if (scheduler is null)
                    {
                        return;
                    }
                    await DoTaskAsync(scheduler, attr);
                }
            });
        }

        private static async Task WaitAsync(Task pollingDelay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(_minIdleTime, stoppingToken);
                await pollingDelay;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}