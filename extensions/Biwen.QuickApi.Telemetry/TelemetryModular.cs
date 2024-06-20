using Biwen.QuickApi.Abstractions.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Telemetry
{
    internal class TelemetryModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {

            // 延迟监控
            services.AddLatencyContext();

            // Add Checkpoints, Measures, Tags
            services.RegisterMeasureNames("responseTime", "processingTime");
            services.RegisterTagNames("userId", "transactionId");
            services.RegisterCheckpointNames(nameof(Biwen.QuickApi));

            // Add Console Latency exporter.
            services.AddConsoleLatencyDataExporter();

            // 自定义的LatencyDataExporter
            // services.AddSingleton<ILatencyDataExporter, QuickApiDataExporter>();

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