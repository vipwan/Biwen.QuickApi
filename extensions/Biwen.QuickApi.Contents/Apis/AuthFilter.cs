// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 23:03:27 AuthFilter.cs

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Biwen.QuickApi.Contents.Apis;

/// <summary>
/// 鉴权筛选器
/// </summary>
internal class AuthFilter(IOptions<BiwenContentOptions> options) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var flag = await options.Value.PermissionValidator(context.HttpContext);

        if (!flag)
            context.HttpContext.Response.StatusCode = 401;

        return await next(context);
    }
}