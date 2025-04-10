// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 22:52:58 ContentsGroupRouteBuilder.cs

using Biwen.QuickApi.Contents.Apis.Filters;
using Biwen.QuickApi.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Biwen.QuickApi.Contents;

/// <summary>
/// 定义文档的GroupRouteBuilder,用于验证等操作
/// </summary>
internal class ContentsGroupRouteBuilder(IOptions<BiwenContentOptions> options) : IQuickApiGroupRouteBuilder
{
    public string Group => Constants.GroupName;

    public int Order => 1000;

    public RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder)
    {
        //添加Tags
        routeBuilder.WithTags([Constants.Tags]);

        //验证权限的Filter
        routeBuilder.AddEndpointFilter<AuthFilter>();

        //是否启用Api:
        if (!options.Value.EnableApi)
        {
            // 如果禁用API，则添加一个拦截所有请求的过滤器
            routeBuilder.AddEndpointFilter(async (context, next) =>
            {
                // 返回403禁止访问
                await Task.CompletedTask;
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            });
        }

        if (!options.Value.GenerateApiDocument)
        {
            routeBuilder.ExcludeFromDescription();
        }

        //不需要验证逻辑,在Biwen.QuickApi中直接处理ValidationException
        //routeBuilder.AddEndpointFilter<ValidationExceptionFilter>();

        //待实现...

        return routeBuilder;
    }
}
