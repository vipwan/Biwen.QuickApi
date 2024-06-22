using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// 提供性能数据输出到OpenTelemetry
    /// </summary>
    internal class OpenTelemetryPublisher : IResourceUtilizationPublisher
    {
        /// <summary>
        /// ActivitySource
        /// </summary>
        static readonly ActivitySource MyActivitySource = new ActivitySource(Constant.OpenTelemetryActivitySourceName);
        //将数据输出到OpenTelemetry:
        static readonly Meter meter = new Meter(Constant.OpenTelemetryActivitySourceName, Constant.OpenTelemetryVersion);

        public static readonly Channel<ResourceUtilization> ResourceUtilizationChannel = Channel.CreateUnbounded<ResourceUtilization>();


        public ValueTask PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
        {
            ResourceUtilizationChannel.Writer.TryWrite(utilization);
            return default;
        }
    }
}