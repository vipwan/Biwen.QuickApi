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
            var md = context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
            if (md == null || md.QuickApiType == null)
            {
                await _next(context);
                return;
            }

            //PoweredBy
            context.Response.Headers.TryAdd("X-Powered-By", AssemblyName);

            var handler = context.RequestServices.GetRequiredService(md.QuickApiType) as IQuickApiMiddlewareHandler;
            if (handler == null)
            {
                await _next(context);
                return;
            }
            await handler.InvokeBeforeAsync(context);
            await _next(context);
            await handler.InvokeAfterAsync(context);
        }
    }
}