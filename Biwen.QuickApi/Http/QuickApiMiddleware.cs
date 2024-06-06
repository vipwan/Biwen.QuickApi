using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// QuickApiMiddleware
    /// </summary>
    public sealed class QuickApiMiddleware
    {
        private static readonly string? AssemblyName = typeof(ServiceRegistration).Assembly.GetName().Name;

        private readonly RequestDelegate _next;
        public QuickApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var isQuickApiEndpoint = context.GetEndpoint()?.Metadata.OfType<QuickApiEndpointMetadata>().Any() is true;
            if (isQuickApiEndpoint)
            {
                context.Response.Headers.TryAdd("X-Powered-By", AssemblyName);
                await _next(context);
                return;
            }

            var md = context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
            if (md is { QuickApiType: not null })
            {
                //PoweredBy
                context.Response.Headers.TryAdd("X-Powered-By", AssemblyName);

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