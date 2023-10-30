using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// 401 403不做跳转而是直接返回StatusCode
    /// </summary>
    internal class QuickApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

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

            //QuickApiAuthorizationMiddlewareResultHandler 直接返回StatusCode
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

            // Fall back to the default implementation.
            return defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}