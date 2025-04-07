// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 22:52:58 ContentsGroupRouteBuilder.cs

using Biwen.QuickApi.Contents.Apis;
using Biwen.QuickApi.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Contents;

/// <summary>
/// 定义文档的GroupRouteBuilder,用于验证等操作
/// </summary>
internal class ContentsGroupRouteBuilder : IQuickApiGroupRouteBuilder
{
    public string Group => Constants.GroupName;

    public int Order => 1000;

    public RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder)
    {
        //添加Tags
        routeBuilder.WithTags([Constants.Tags]);

        //验证的Filter
        routeBuilder.AddEndpointFilter<AuthFilter>();

        //验证传递的Content
        //待实现...

        return routeBuilder;
    }
}
