// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:43 MiniProfilerOptions.cs

using StackExchange.Profiling;

namespace Biwen.QuickApi.MiniProfiler;

/// <summary>
/// MiniProfiler配置选项
/// </summary>
public class WrapMiniProfilerOptions : MiniProfilerOptions
{
    /// <summary>
    /// 配置文件中的定位Key
    /// </summary>
    public const string Key = "BiwenQuickApi:MiniProfiler";

    /// <summary>
    /// 是否启用MiniProfiler
    /// </summary>
    public bool Enabled { get; set; } = false;

}