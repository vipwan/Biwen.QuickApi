using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// QuickApiMiddleware
    /// </summary>
    public sealed class QuickApiMiddleware
    {
        private static readonly string? AssemblyName = typeof(ServiceRegistration).Assembly.GetName().Name;
        private static readonly string version = $"{typeof(ServiceRegistration).Assembly.GetName().Version}";

        private readonly RequestDelegate _next;
        public QuickApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var md = context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
            if (md == null || md.QuickApiType == null)
            {
                await _next(context);
                return;
            }

            //PoweredBy
            context.Response.Headers.XPoweredBy = AssemblyName;
            //Version
            context.Response.Headers.TryAdd($"X-{nameof(QuickApi)}-Version", version);

            var handler = context.RequestServices.GetRequiredService(md.QuickApiType) as IQuickApiMiddlewareHandler;
            if (handler == null)
            {
                await _next(context);
                return;
            }
            await handler.BeforeAsync(context);
            await _next(context);
            await handler.AfterAsync(context);
        }
    }
}