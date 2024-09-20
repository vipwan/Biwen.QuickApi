// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:19 CoreInstrumentation.cs

using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Biwen.QuickApi.Telemetry.Implementation;

/// <summary>
/// 消费性能监控
/// </summary>
internal class CoreInstrumentation : IAsyncDisposable
{
    /// <summary>
    /// ActivitySource
    /// </summary>
    static readonly ActivitySource MyActivitySource = new ActivitySource(Constant.OpenTelemetryActivitySourceName);
    //将数据输出到OpenTelemetry:
    static readonly Meter meter = new Meter(Constant.OpenTelemetryActivitySourceName, Constant.OpenTelemetryVersion);

    public CoreInstrumentation()
    {
        Task.Run(async () =>
        {
            // Create a counter
            var counter = meter.CreateCounter<long>($"{Constant.Prefix}InvokedCount", "counter", description: "性能监控推送次数");

#pragma warning disable CS0618 // 类型或成员已过时
            await foreach (var utilization in OpenTelemetryPublisher.ResourceUtilizationChannel.Reader.ReadAllAsync())
            {
                PublishAsync(utilization, CancellationToken.None);
                counter.Add(1);
            }
#pragma warning restore CS0618 // 类型或成员已过时
        });
    }

    public void PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
    {
        meter.CreateObservableGauge($"{Constant.Prefix}{Constant.CpuUsedPercentage}", () => utilization.CpuUsedPercentage, description: "CPU使用百分比%");
        meter.CreateObservableGauge($"{Constant.Prefix}{Constant.MemoryUsedInBytes}", () => (double)utilization.MemoryUsedInBytes, description: "内存使用量bytes");
        meter.CreateObservableGauge($"{Constant.Prefix}{Constant.MemoryUsedPercentage}", () => utilization.MemoryUsedPercentage, description: "内存使用百分比%");
        meter.CreateObservableGauge($"{Constant.Prefix}{Constant.MaximumMemoryInBytes}", () => (double)utilization.SystemResources.MaximumMemoryInBytes, description: "服务器最大内存bytes");

        //using (var activity = MyActivitySource.StartActivity("Activity1"))
        //{
        //    activity?.Start();
        //}
    }

    public ValueTask DisposeAsync()
    {
        try
        {
#pragma warning disable CS0618 // 类型或成员已过时
            OpenTelemetryPublisher.ResourceUtilizationChannel?.Writer?.Complete();
#pragma warning restore CS0618 // 类型或成员已过时
        }
        catch
        {
            //todo:
        }
        return default;
    }
}
