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
            //IQuickEndPoint
            if (context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>() is { } ||
                context.GetEndpoint()?.Metadata.GetMetadata<QuickApiEndpointMetadata>() is { })
            {
                if (authorizeResult.Challenged || authorizeResult.Forbidden)
                {
                    context.Response.StatusCode = authorizeResult switch
                    {
                        { Challenged: true } => 401,
                        { Forbidden: true } => 403,
                        _ => context.Response.StatusCode
                    };
                    return Task.CompletedTask;
                }
            }

            // Fall back to the default implementation.
            return defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}