using Biwen.QuickApi.Auditing;
using Biwen.QuickApi.Auditing.Abstractions;
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
                try
                {
                    if (httpContext.GetEndpoint()?.Metadata.GetMetadata<AuditApiAttribute>() is { })
                    {
                        var handlers = httpContext.RequestServices.GetService<IEnumerable<IAuditHandler>>()!;
                        var auditInfo = new AuditInfo
                        {
                            ApplicationName = "Biwen.QuickApi",
                            UserId = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                            UserName = httpContext.User.Identity?.Name,
                            BrowserInfo = httpContext.Request.Headers.UserAgent,
                            ClientIpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                            ClientName = httpContext.Request.Headers["X-Forwarded-For"],
                            HttpMethod = httpContext.Request.Method,
                            Url = httpContext.Request.Path,
                            IsQuickApi = true,//表明这是一个QuickApi请求
                            ExtraInfos = new Dictionary<string, object?>()
                            {
                                ["args"] = context.Arguments,
                                ["result"] = httpContext.Response.Body,
                                ["headers"] = httpContext.Response.Headers,
                                ["query"] = httpContext.Request.Query,
                                ["cookies"] = httpContext.Request.Cookies,
                            }
                        };

                        foreach (var handler in handlers)
                        {
                            //审计不要阻塞业务
                            _ = handler.Handle(auditInfo);
                        }
                    }
                }
                catch
                {
                    //审计功能不能影响业务
                    //todo:
                }
            }

            return @delegate;
        }
    }
}
