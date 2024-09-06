// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:32 LatencyContextControlExtensions.cs

using Microsoft.Extensions.Diagnostics.Latency;

namespace Biwen.QuickApi.Telemetry
{
    internal static class LatencyContextControlExtensions
    {
        /// <summary>
        /// 获取指定名称的检查点
        /// </summary>
        /// <param name="latencyContext"></param>
        /// <param name="checkpointName"></param>
        /// <param name="elapsed"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static bool TryGetCheckpoint(this ILatencyContext latencyContext, string checkpointName, out long elapsed, out long frequency)
        {
            var checkpoints = latencyContext.LatencyData.Checkpoints;
            foreach (var checkpoint in checkpoints)
            {
                if (string.Equals(checkpoint.Name, checkpointName, StringComparison.Ordinal))
                {
                    elapsed = checkpoint.Elapsed;
                    frequency = checkpoint.Frequency;
                    return true;
                }
            }

            elapsed = 0;
            frequency = 0;
            return false;
        }
    }
}
