// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:45 SpectreConsolePublisher.cs

using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// 提供性能数据输出到控制台
    /// </summary>
    [Obsolete("未来版本将停用!", false)]
    internal class SpectreConsolePublisher(IOptions<ResourceMonitoringOptions> options) : IResourceUtilizationPublisher
    {
        public ValueTask PublishAsync(ResourceUtilization utilization, CancellationToken cancellationToken)
        {
            //使用Spectre.Console展示输出:
            var table = new Table()
            .Centered()
            .Title("Resource Monitoring", new Style(foreground: Color.Purple, decoration: Decoration.Bold))
            .Caption($"Updates every {options.Value.SamplingInterval.TotalSeconds} seconds. *GTD: Guaranteed ", new Style(decoration: Decoration.Dim))
            .RoundedBorder()
            .BorderColor(Color.Cyan1)
            .AddColumns([
                new TableColumn("Time").Centered(),
                new TableColumn("CPU %").Centered(),
                new TableColumn("Memory %").Centered(),
                new TableColumn("Memory (bytes)").Centered(),
                new TableColumn("GTD / Max Memory (bytes)").Centered(),
                new TableColumn("GTD / Max CPU (units)").Centered(),
            ]);

            var resources = utilization.SystemResources;

            table.AddRow([
                    $"{DateTime.Now:T}",
                    $"{utilization.CpuUsedPercentage :p}",
                    $"{utilization.MemoryUsedPercentage / 100 :p}",
                    $"{utilization.MemoryUsedInBytes:#,#}",
                    $"{resources.GuaranteedMemoryInBytes:#,#} / {resources.MaximumMemoryInBytes:#,#}",
                    $"{resources.GuaranteedCpuUnits} / {resources.MaximumCpuUnits}",
             ]);

            AnsiConsole.Write(table);
            return default;
        }
    }
}