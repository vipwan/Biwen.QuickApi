// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:28 IQuickApiGroupRouteBuilder.cs

using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Http;

/// <summary>
/// Group HanderBuilder
/// </summary>
public interface IQuickApiGroupRouteBuilder
{
    /// <summary>
    /// 分组
    /// </summary>
    string Group { get; }

    RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder);

    /// <summary>
    /// 执行顺序
    /// </summary>
    int Order { get; }
}

/// <summary>
/// DI: services.AddQuickApiGroupRouteBuilder<QuickApiGroupRouteBuilder>();
/// </summary>
public abstract class BaseQuickApiGroupRouteBuilder : IQuickApiGroupRouteBuilder
{
    public abstract string Group { get; }

    public abstract int Order { get; }

    public virtual RouteGroupBuilder Builder(RouteGroupBuilder builder)
    {
        return builder;
    }
}