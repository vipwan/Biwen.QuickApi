using Biwen.QuickApi.Auditing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Biwen.QuickApi.Http
{
    internal class AuditApiFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var @delegate = await next(context);

            if (context.HttpContext is { } httpContext)
            {
                var audited = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<AuditApiAttribute>() is { };
                if (audited)
                {
                    var handlers = httpContext.RequestServices.GetService<IEnumerable<IAuditHandler>>()!;
                    var auditInfo = new AuditInfo
                    {
                        ApplicationName = "Biwen.QuickApi",
                        UserId = httpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                        UserName = httpContext?.User.Identity?.Name,
                        BrowserInfo = httpContext?.Request.Headers.UserAgent,
                        ClientIpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                        ClientName = httpContext?.Request.Headers["X-Forwarded-For"],
                        HttpMethod = httpContext?.Request.Method,
                        Url = httpContext?.Request.Path,
                        IsQuickApi = true,//表明这是一个QuickApi请求
                        ExtraInfos = new Dictionary<string, object?>()
                        {
                            ["args"] = context.Arguments,
                            ["result"] = context.HttpContext.Response.Body,
                            ["headers"] = context.HttpContext.Response.Headers,
                            ["query"] = context.HttpContext.Request.Query,
                            ["cookies"] = context.HttpContext.Request.Cookies,
                        }
                    };

                    foreach (var handler in handlers)
                    {
                        //审计不要阻塞业务
                        _ = handler.Handle(auditInfo);
                    }
                }
            }

            return @delegate;
        }
    }
}
