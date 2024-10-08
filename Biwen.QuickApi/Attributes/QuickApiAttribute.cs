﻿// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 QuickApiAttribute.cs

using System.Diagnostics.CodeAnalysis;

namespace Biwen.QuickApi.Attributes;

/// <summary>
/// QuickApi特性
/// </summary>

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class QuickApiAttribute : Attribute
{
    public QuickApiAttribute([StringSyntax("Route")] string route)
    {
        ArgumentNullException.ThrowIfNull(route);
        Route = route;
    }
    /// <summary>
    /// 分组. 例如: hello,不可为Null
    /// </summary>
    public string Group { get; set; } = string.Empty;
    /// <summary>
    /// 路由. 例如: hello/world/{name}
    /// </summary>
    public string Route { get; set; } = null!;
    /// <summary>
    /// 请求方式. 默认为GET, 如果需要多种方式，可以使用 | 运算符
    /// </summary>
    public Verb Verbs { get; set; } = Verb.GET;
    /// <summary>
    /// 请求策略. 默认为null，表示不需要验证
    /// 推荐使用HandlerBuilder的方式实现强大的鉴权功能
    /// </summary>
    public string? Policy { get; set; }
}