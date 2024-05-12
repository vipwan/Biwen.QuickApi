using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// 用于检查Policy的筛选器
    /// </summary>
    internal class CheckPolicyFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var httpContext = context.HttpContext;
            if (httpContext != null)
            {
                var quickApiMetadata = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
                if (quickApiMetadata is QuickApiMetadata meta && meta.QuickApiAttribute != null)
                {
                    var (flag, result) = await CheckPolicy(httpContext, meta.QuickApiAttribute.Policy);
                    if (!flag)
                    {
                        return result;
                    }
                }
            }
            return await next(context);
        }

        /// <summary>
        /// 验证Policy
        /// </summary>
        /// <exception cref="QuickApiExcetion"></exception>
        async static Task<(bool Flag, IResult? Result)> CheckPolicy(HttpContext ctx, string? policy)
        {
            if (string.IsNullOrEmpty(policy))
            {
                return (true, null);
            }
            if (!string.IsNullOrEmpty(policy))
            {
                var authService = ctx!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null, besure services.AddAuthorization() first!");
                var authorizationResult = await authService.AuthorizeAsync(ctx.User, policy);
                if (!authorizationResult.Succeeded)
                {
                    return (false, TypedResults.Unauthorized());
                }
            }
            return (true, null);
        }

    }
}
