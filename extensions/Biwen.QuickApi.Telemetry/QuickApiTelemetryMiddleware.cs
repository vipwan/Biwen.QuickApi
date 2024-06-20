using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Latency;
using System.Diagnostics;

namespace Biwen.QuickApi.Telemetry
{
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