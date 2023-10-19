using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
namespace Biwen.QuickApi.Http
{

    /// <summary>
    /// QuickApiMiddleware
    /// </summary>
    public sealed class QuickApiAntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public QuickApiAntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task Invoke(HttpContext context)
        {
            //GET请求不需要防伪验证
            if(context.Request.Method == HttpMethods.Get)
            {
                await _next(context);
                return;
            }

#if NET8_0_OR_GREATER
            var requiresValidation = context.GetEndpoint()?.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation;
            //.NET8支持屏蔽防伪验证
            if (requiresValidation is false)
            {
                await _next(context);
                return;
            }
#endif
            var md = context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
            if (md == null || md.QuickApiType == null)
            {
                await _next(context);
                return;
            }
            var antiforgeryApi = context.RequestServices.GetRequiredService(md.QuickApiType) as IAntiforgeryApi;
            if (antiforgeryApi?.IsAntiforgeryEnabled is true)
            {
                try
                {
                    await _antiforgery.ValidateRequestAsync(context);
                }
                catch (AntiforgeryValidationException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid anti-forgery token");
                    return;
                }
            }
            await _next(context);
        }
    }
}