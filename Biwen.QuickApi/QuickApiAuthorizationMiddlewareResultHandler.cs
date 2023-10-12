using Microsoft.AspNetCore.Authentication;
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

            //AuthorizationMiddlewareResultHandler 原始实现
            return Handle();
            async Task Handle()
            {
                if (authorizeResult.Challenged)
                {
                    if (policy.AuthenticationSchemes.Count > 0)
                    {
                        foreach (var scheme in policy.AuthenticationSchemes)
                        {
                            await context.ChallengeAsync(scheme);
                        }
                    }
                    else
                    {
                        await context.ChallengeAsync();
                    }
                }
                else if (authorizeResult.Forbidden)
                {
                    if (policy.AuthenticationSchemes.Count > 0)
                    {
                        foreach (var scheme in policy.AuthenticationSchemes)
                        {
                            await context.ForbidAsync(scheme);
                        }
                    }
                    else
                    {
                        await context.ForbidAsync();
                    }
                }
            }
        }
    }
}