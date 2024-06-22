using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Latency;

namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// QuickApi性能监控中间件
    /// </summary>
    [Obsolete("OpenTelemetry.Instrumentation.AspNetCore库已经集成,无需二次开发", false)]
    internal class QuickApiTelemetryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILatencyContextTokenIssuer _issuer;

        public QuickApiTelemetryMiddleware(RequestDelegate next, ILatencyContextTokenIssuer tokenIssuer)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _issuer = tokenIssuer ?? throw new ArgumentNullException(nameof(tokenIssuer));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var latencyContext = context.RequestServices.GetRequiredService<ILatencyContext>();
            //var stopwatch = Stopwatch.StartNew();
            context.Response.OnStarting(ctx =>
            {
                var httpContext = (HttpContext)ctx;
                var latencyContext = httpContext.RequestServices.GetRequiredService<ILatencyContext>();
                latencyContext.AddCheckpoint(_issuer.GetCheckpointToken("startTime"));
                return Task.CompletedTask;

            }, latencyContext);

            context.Response.OnCompleted(ctx =>
            {
                var httpContext = (HttpContext)ctx;
                var latencyContext = httpContext.RequestServices.GetRequiredService<ILatencyContext>();
                latencyContext.AddCheckpoint(_issuer.GetCheckpointToken("endTime"));
                return Task.CompletedTask;

            }, latencyContext);

            await _next(context);
        }
    }
}