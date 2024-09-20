// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:50 TelemetryModular.cs

using Biwen.QuickApi.Abstractions.Modular;
using Biwen.QuickApi.Telemetry.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.CodeAnalysis;

namespace Biwen.QuickApi.Telemetry;

[ExcludeFromCodeCoverage]
internal class TelemetryModular(IConfiguration configuration) : ModularBase
{
    /// <summary>
    /// 根据配置项判断是否启用
    /// </summary>
    public override Func<bool> IsEnable => () =>
    {
        var flag = configuration.GetValue<bool?>($"{TelemetryOptions.Key}:{nameof(TelemetryOptions.Enable)}");
        return flag is true;
    };

    public override void ConfigureServices(IServiceCollection services)
    {

        // OpenTelemetry
        var openTelemetryBuilder = services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                var env = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();
                resource.AddService(env.ApplicationName, serviceVersion: Constant.OpenTelemetryVersion);
            })
            .WithMetrics(builder =>
            {
                // Add ASP.NET Core Instrumentation
                builder.AddAspNetCoreInstrumentation();//AspnetCore指标
                builder.AddHttpClientInstrumentation();//HttpClient指标
                builder.AddProcessInstrumentation();//应用的进程指标

                // Add Processor
                builder.AddMeter(Constant.OpenTelemetryActivitySourceName);

                // Metrics provides by ASP.NET Core in .NET 8
                builder.AddMeter("Microsoft.AspNetCore.Hosting");
                builder.AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                builder.AddMeter(SchedulingInstrumentation.SourceName);

                //builder.AddConsoleExporter();//导出到控制台
                builder.AddPrometheusExporter();

            })
            .WithLogging()

            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();

                tracing.AddSource(Constant.OpenTelemetryActivitySourceName);
                tracing.AddSource(SchedulingInstrumentation.SourceName);

                //tracing.AddConsoleExporter()//导出到控制台
            });


        // UseOtlpExporter
        openTelemetryBuilder.UseOtlpExporter();

        //性能监控
        services.AddResourceMonitoring(o =>
        {
            o.ConfigureMonitor(monitor =>
            {
                var SamplingInterval = configuration.GetValue<double?>($"{TelemetryOptions.Key}:{nameof(TelemetryOptions.SamplingInterval)}");
                monitor.SamplingInterval = TimeSpan.FromSeconds(SamplingInterval ?? 15d);
            });

            //添加SpectreConsolePublisher
#pragma warning disable
            o.AddPublisher<SpectreConsolePublisher>();

            //添加OpenTelemetryPublisher
            o.AddPublisher<OpenTelemetryPublisher>();
#pragma warning restore

        });

        //消费性能监控数据到OpenTelemetry
        services.AddActivatedSingleton<CoreInstrumentation>();

        //// 延迟监控
        //services.AddLatencyContext();

        //// Add Checkpoints, Measures, Tags
        //services.RegisterMeasureNames("responseTime", "processingTime");
        //services.RegisterTagNames("userId", "transactionId");
        //services.RegisterCheckpointNames(nameof(Biwen.QuickApi));

        //// Add Console Latency exporter.
        //services.AddConsoleLatencyDataExporter();

        //// 自定义的LatencyDataExporter
        //// services.AddSingleton<ILatencyDataExporter, QuickApiDataExporter>();

        //// Add Request latency telemetry.
        //services.AddRequestLatencyTelemetry();
        //// Add Request Checkpoint
        //services.AddRequestCheckpoint();

    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {

        routes.MapPrometheusScrapingEndpoint();

        //// Add Request Latency Middleware which will automatically call ExportAsync on all registered latency exporters.
        //app.UseRequestLatencyTelemetry();
        //// Add Request Checkpoint Middleware which will automatically call ExportAsync on all registered checkpoint exporters.
        //app.UseRequestCheckpoint();
    }
}