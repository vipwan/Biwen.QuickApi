using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Biwen.QuickApi.Telemetry.Implementation
{
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
                await foreach (var utilization in OpenTelemetryPublisher.ResourceUtilizationChannel.Reader.ReadAllAsync())
                {
                    PublishAsync(utilization, CancellationToken.None);
                }
            });
        }

        public void PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
        {

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

            using (var activity = MyActivitySource.StartActivity("Activity1"))
            {
                counter.Add(1);
                activity?.Start();
            }
        }

        public ValueTask DisposeAsync()
        {
            OpenTelemetryPublisher.ResourceUtilizationChannel?.Writer.Complete();
            return default;
        }
    }
}
