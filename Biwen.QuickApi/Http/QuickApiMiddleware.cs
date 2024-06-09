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
            var addHeader = () =>
            {
                //PoweredBy
                context.Response.Headers.XPoweredBy = AssemblyName;
                //Version
                context.Response.Headers.TryAdd($"X-{nameof(QuickApi)}-Version", version);
            };

            if (context.GetEndpoint()?.Metadata.OfType<QuickApiEndpointMetadata>().Any() is true)
            {
                addHeader();
                await _next(context);
                return;
            }

            if (context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>() is { QuickApiType: not null } md)
            {
                addHeader();
                if (context.RequestServices.GetService(md.QuickApiType) is IQuickApiMiddlewareHandler handler)
                {
                    await handler.BeforeAsync(context);
                    await _next(context);
                    await handler.AfterAsync(context);
                    return;
                }
            }

            await _next(context);
        }
    }
}