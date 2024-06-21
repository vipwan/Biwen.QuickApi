using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// 提供性能数据输出到控制台
    /// </summary>
    internal class OpenTelemetryPublisher : IResourceUtilizationPublisher
    {
        public ValueTask PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
        {

            //将数据输出到OpenTelemetry:
            var meter = new Meter(Constant.OpenTelemetryActivitySourceName, Constant.OpenTelemetryVersion);

            // Create a counter
            var counter = meter.CreateCounter<long>("InvokedCount", "counter", description: "性能监控推送次数");

            meter.CreateObservableGauge(Constant.CpuUsedPercentage, () => utilization.CpuUsedPercentage,
                "gauge", "CPU使用百分比%");
            meter.CreateObservableGauge(Constant.MemoryUsedInBytes, () => (double)utilization.MemoryUsedInBytes,
                "gauge", "内存使用量bytes");
            meter.CreateObservableGauge(Constant.MemoryUsedPercentage, () => utilization.MemoryUsedPercentage,
                "gauge", "内存使用百分比%");
            meter.CreateObservableGauge(Constant.MaximumMemoryInBytes, () => (double)utilization.SystemResources.MaximumMemoryInBytes,
                "gauge", "服务器最大内存bytes");

            using var activitySource1 = new ActivitySource(Constant.OpenTelemetryActivitySourceName);

            using (var activity = activitySource1.StartActivity("Activity1"))
            {
                counter.Add(1);
                activity?.Start();
            }

            return default;
        }
    }
}