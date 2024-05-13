using Biwen.QuickApi.Scheduling.Store;
using Microsoft.Extensions.Hosting;
namespace Biwen.QuickApi.Scheduling
{
    internal class ScheduleBackgroundService : BackgroundService
    {
#if DEBUG
        //轮询20s 测试环境下，方便测试。
        private static readonly TimeSpan _pollingTime = TimeSpan.FromSeconds(20);
#endif
#if !DEBUG
        //轮询45s 正式环境下，考虑性能轮询时间延长到45s
        private static readonly TimeSpan _pollingTime = TimeSpan.FromSeconds(45);
#endif

        //延时10s,出于性能考虑,这里不做太频繁的轮询.
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
                }
            }

            //store中的scheduler
            var stores = _serviceProvider.GetServices<IScheduleMetadaStore>().ToArray();

            foreach (var store in stores)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                var metadatas = await store.GetAllAsync();

                if (metadatas is null || !metadatas.Any())
                {
                    continue;
                }

                foreach (var metadata in metadatas)
                {
                    var attr = new ScheduleTaskAttribute(metadata.Cron)
                    {
                        Description = metadata.Description,
                        IsAsync = metadata.IsAsync,
                        IsStartOnInit = metadata.IsStartOnInit,
                    };

                    var timeScheduler = new BackgroundTaskScheduler(attr, DateTime.Now);
                    if (timeScheduler.CanRun())
                    {
                        var scheduler = scope.ServiceProvider.GetRequiredService(metadata.ScheduleTaskType) as IScheduleTask;
                        if (scheduler is null)
                        {
                            continue;
                        }
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
                }
            }
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