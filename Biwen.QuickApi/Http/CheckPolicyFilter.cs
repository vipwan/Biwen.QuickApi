// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:06 CheckPolicyFilter.cs

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
                if (quickApiMetadata is QuickApiMetadata { QuickApiAttribute: { } })
                {
                    var (flag, result) = await CheckPolicyAsync(httpContext, quickApiMetadata.QuickApiAttribute.Policy);
                    if (!flag)
                    {
                        return result;
                    }
                }
                else if (quickApiMetadata is QuickApiMetadata { QuickApiAttribute: null })
                {
                    var attr = quickApiMetadata.QuickApiType!.GetCustomAttribute<QuickApiAttribute>() ?? throw new QuickApiExcetion($"{quickApiMetadata.QuickApiType!.Name}:必须标注QuickApi特性!");
                    var (flag, result) = await CheckPolicyAsync(httpContext, attr.Policy);
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
        async static Task<(bool Flag, IResult? Result)> CheckPolicyAsync(HttpContext ctx, string? policy)
        {
            if (string.IsNullOrWhiteSpace(policy))
            {
                return (true, null);
            }

            var authService = ctx!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null, besure services.AddAuthorization() first!");
            var authorizationResult = await authService.AuthorizeAsync(ctx.User, policy);
            if (!authorizationResult.Succeeded)
            {
                return (false, TypedResults.Unauthorized());
            }

            return (true, null);
        }

    }
}