﻿// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:53:26 ScheduleBackgroundService.cs

using Biwen.QuickApi.Caching.Abstractions;
using Biwen.QuickApi.Events;
using Biwen.QuickApi.Infrastructure.Locking;
using Biwen.QuickApi.Scheduling.Events;
using Biwen.QuickApi.Scheduling.Stores;
using Microsoft.Extensions.Hosting;

namespace Biwen.QuickApi.Scheduling;

/// <summary>
/// ScheduleBackgroundService
/// </summary>
internal class ScheduleBackgroundService : BackgroundService
{
    // Init the delay first to be also waited on exception.
    private static readonly TimeSpan _pollingTime = TimeSpan.FromSeconds(5);
    //10秒轮询,意味着最小的秒级支持10秒+
    private static readonly TimeSpan _minIdleTime = TimeSpan.FromSeconds(5);

    private readonly ILogger<ScheduleBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILocalLock _localLock;

    public ScheduleBackgroundService(
        ILogger<ScheduleBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ILocalLock localLock)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
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
        using var scope = _serviceScopeFactory.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        //调度器
        var scheduler = ActivatorUtilities.GetServiceOrCreateInstance<IScheduler>(sp);

        var cachingProxyFactory = ActivatorUtilities.GetServiceOrCreateInstance<CachingProxyFactory<IScheduleMetadataStore>>(sp);
        var proxyCache = ActivatorUtilities.GetServiceOrCreateInstance<IProxyCache>(sp);

        async Task DoTaskAsync(IScheduleTask task, ScheduleTaskAttribute metadata)
        {
            //内部执行
            async Task InnerExcute()
            {
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
                    await onlyOneRunningScheduleTask.OnAbortAsync();
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
        var stores = sp.GetServices<IScheduleMetadataStore>().ToArray();

        //并行执行,提高性能
        Parallel.ForEach(stores, async store =>
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            //获取所有的元数据,使用缓存代理
            if (await cachingProxyFactory.Create(store, proxyCache).GetAllAsync() is not { Length: > 0 } metadatas)
            {
                return;
            }

            foreach (var metadata in metadatas)
            {
                using var scope2 = _serviceScopeFactory.CreateScope();
                var sp2 = scope2.ServiceProvider;

                if (ActivatorUtilities.GetServiceOrCreateInstance(sp2, metadata.ScheduleTaskType)
                as IScheduleTask is { } task)
                {
                    var attr = new ScheduleTaskAttribute(metadata.Cron)
                    {
                        Description = metadata.Description,
                        IsAsync = metadata.IsAsync,
                        IsStartOnInit = metadata.IsStartOnInit,
                    };
                    await DoTaskAsync(task, attr);
                }
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