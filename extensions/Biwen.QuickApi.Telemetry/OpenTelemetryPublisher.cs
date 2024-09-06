// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:36 OpenTelemetryPublisher.cs

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