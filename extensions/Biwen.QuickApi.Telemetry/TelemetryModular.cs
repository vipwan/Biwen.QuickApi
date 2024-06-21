﻿using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Biwen.QuickApi.Telemetry
{
    internal class TelemetryModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {

            // OpenTelemetry
            var openTelemetryBuilder = services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    var env = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();
                    resource.AddService(env.ApplicationName);
                })
                .WithMetrics(builder =>
                {
                    // Add ASP.NET Core Instrumentation
                    builder.AddAspNetCoreInstrumentation();
                    builder.AddHttpClientInstrumentation();

                    // Add Processor
                    builder.AddMeter(Constant.OpenTelemetryActivitySourceName);

                    // Metrics provides by ASP.NET Core in .NET 8
                    builder.AddMeter("Microsoft.AspNetCore.Hosting");
                    builder.AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                    //builder.AddConsoleExporter();//导出到控制台
                    builder.AddPrometheusExporter();

                })
                .WithLogging()

                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation();
                    tracing.AddHttpClientInstrumentation();

                    tracing.AddSource(Constant.OpenTelemetryActivitySourceName);

                    //tracing.AddConsoleExporter()//导出到控制台
                });


            // UseOtlpExporter
            openTelemetryBuilder.UseOtlpExporter();



            //性能监控
            services.AddResourceMonitoring(o =>
            {
                o.ConfigureMonitor(monitor =>
                {
                    monitor.SamplingInterval = TimeSpan.FromSeconds(15d);
                });

                //添加SpectreConsolePublisher
                o.AddPublisher<SpectreConsolePublisher>();

                //添加OpenTelemetryPublisher
                o.AddPublisher<OpenTelemetryPublisher>();

            });


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
}