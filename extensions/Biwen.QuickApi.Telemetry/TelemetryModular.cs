using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Latency;

namespace Biwen.QuickApi.Telemetry
{
    internal class TelemetryModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceLogEnricher(options =>
            {
                options.ApplicationName = true;//default is true
                options.EnvironmentName = true;//default is true

                options.BuildVersion = true;
                options.DeploymentRing = true;
            });

            // 进程日志的Enrich
            services.AddProcessLogEnricher(options =>
            {
                options.ProcessId = true;
                options.ThreadId = true;
            });

            services.AddRequestHeadersLogEnricher();

            // QuickApi的Enrich
            services.AddStaticLogEnricher<QuickApiLogEnricher>();

            // 延迟监控
            services.AddLatencyContext();

            // Add Checkpoints, Measures, Tags
            services.RegisterMeasureNames("responseTime", "processingTime");
            services.RegisterTagNames("userId", "transactionId");
            services.RegisterCheckpointNames(nameof(Biwen.QuickApi));

            // Add Console Latency exporter.
            services.AddConsoleLatencyDataExporter(options =>
            {
                options.OutputCheckpoints = true;
                options.OutputMeasures = true;
                options.OutputTags = true;
            });

            // Optionally add custom exporters.
            services.AddSingleton<ILatencyDataExporter, QuickApiDataExporter>();

            // Add Request latency telemetry.
            services.AddRequestLatencyTelemetry();
            // Add Request Checkpoint
            services.AddRequestCheckpoint();

        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Add Request Latency Middleware which will automatically call ExportAsync on all registered latency exporters.
            app.UseRequestLatencyTelemetry();
            // Add Request Checkpoint Middleware which will automatically call ExportAsync on all registered checkpoint exporters.
            app.UseRequestCheckpoint();
        }
    }
}