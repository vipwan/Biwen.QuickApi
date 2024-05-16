using Biwen.QuickApi.Events;
using Biwen.QuickApi.Infrastructure.Locking;
using Biwen.QuickApi.Scheduling.Events;
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
        private readonly ILocalLock _localLock;

        public ScheduleBackgroundService(
            ILogger<ScheduleBackgroundService> logger,
            IServiceProvider serviceProvider,
            ILocalLock localLock)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _localLock = localLock;
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

        private Task RunAsync(CancellationToken stoppingToken)
        {
            async Task DoTaskAsync(IScheduleTask task, ScheduleTaskAttribute metadata)
            {
                using var scope = _serviceProvider.CreateScope();
                //内部执行
                async Task InnerExcute()
                {
                    //调度器
                    var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
                    if (scheduler.CanRun(metadata, DateTime.Now))
                    {
                        var eventTime = DateTime.Now;
                        //通知启动
                        _ = new TaskStartedEvent(task, eventTime).PublishAsync(default);
                        try
                        {
                            if (metadata.IsAsync)
                            {
                                //异步执行
                                _ = task.ExecuteAsync();
                            }
                            else
                            {
                                //同步执行
                                await task.ExecuteAsync();
                            }
                            //执行完成
                            _ = new TaskSuccessedEvent(task, eventTime, DateTime.Now).PublishAsync(default);
                        }
                        catch (Exception ex)
                        {
                            _ = new TaskFailedEvent(task, DateTime.Now, ex).PublishAsync(default);
                        }
                    }
                }
                //同一时间只能存在一个的任务
                if (task is OnlyOneRunningScheduleTask onlyOneRunningScheduleTask)
                {
                    var (locker, _) = await _localLock.TryAcquireLockAsync(
                        $"{onlyOneRunningScheduleTask.GetType().FullName}_LOCALLOCK", TimeSpan.FromSeconds(15d));

                    if (locker is null)
                    {
                        //如果有正在运行的相同任务,打断当前的执行的回调
                        await onlyOneRunningScheduleTask.OnAbort();
                        return;
                    }
                    using (locker)
                    {
                        await InnerExcute();
                    }
                }
                else
                {
                    await InnerExcute();
                }
            };

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
                    using var scope = _serviceProvider.CreateScope();
                    var task = scope.ServiceProvider.GetRequiredService(metadata.ScheduleTaskType) as IScheduleTask;
                    if (task is null)
                    {
                        continue;
                    }
                    await DoTaskAsync(task, attr);
                }
            });

            return Task.CompletedTask;
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