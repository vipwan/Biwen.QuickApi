using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace Biwen.QuickApi.OpenApi
{
    /// <summary>
    /// 返回application/json,避免中文乱码
    /// https://github.com/dotnet/aspnetcore/issues/56095
    /// </summary>
    internal sealed class OpenApiDocMiddleware
    {
        private readonly RequestDelegate _next;
        public OpenApiDocMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //如果是openapi document:
            if (context.GetEndpoint()?.Metadata.GetMetadata<IRouteDiagnosticsMetadata>() is { } route &&
                route.Route.EndsWith("{documentName}.json"))
            {
                context.Response.ContentType = "application/json";
            }
            await _next(context);
        }
    }
}