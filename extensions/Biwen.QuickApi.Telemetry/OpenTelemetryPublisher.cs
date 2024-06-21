using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// 提供性能数据输出到控制台
    /// </summary>
    internal class OpenTelemetryPublisher() : IResourceUtilizationPublisher
    {
        public ValueTask PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
        {
            using var activitySource1 = new ActivitySource(Constant.ActivitySourceName);

            //将数据输出到OpenTelemetry:
            var meter = new Meter(Constant.ActivitySourceName, "1.0.0");

            // Create a counter
            var counter = meter.CreateCounter<long>("InvokedCount");

            meter.CreateObservableGauge(Constant.CpuUsedPercentage, () => utilization.CpuUsedPercentage);
            meter.CreateObservableGauge(Constant.MemoryUsedInBytes, () => (double)utilization.MemoryUsedInBytes);
            meter.CreateObservableGauge(Constant.MemoryUsedPercentage, () => utilization.MemoryUsedPercentage);
            meter.CreateObservableGauge(Constant.MaximumMemoryInBytes, () => (double)utilization.SystemResources.MaximumMemoryInBytes);

            using (var activity = activitySource1.StartActivity("Activity1"))
            {
                counter.Add(1);
                activity?.Start();
            }

            return default;
        }
    }
}