using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{

    /// <summary>
    /// 401 403不做跳转而是直接返回StatusCode
    /// </summary>
    internal class QuickApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Succeeded)
            {
                return next(context);
            }

            if (context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>() != null)
            {
                if (authorizeResult.Challenged)
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                else if (authorizeResult.Forbidden)
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }
            }

            return next(context);
        }
    }
}