using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using System.Threading.Channels;

namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// 提供性能数据输出到OpenTelemetry
    /// </summary>
    internal class OpenTelemetryPublisher : IResourceUtilizationPublisher
    {
        public static readonly Channel<ResourceUtilization> ResourceUtilizationChannel = Channel.CreateUnbounded<ResourceUtilization>();

        public ValueTask PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
        {
            ResourceUtilizationChannel.Writer.TryWrite(utilization);
            return default;
        }
    }
}