// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:01:54 TelemetryOptions.cs

namespace Biwen.QuickApi.Telemetry;

/// <summary>
/// TelemetryOptions 配置项
/// </summary>
public class TelemetryOptions
{

    public const string Key = "BiwenQuickApi:Telemetry";

    /// <summary>
    /// 是否启用 默认: false
    /// </summary>
    public bool Enable { get; set; } = false;


    /// <summary>
    /// 服务监控采样间隔 默认: 15s
    /// </summary>
    public uint SamplingInterval { get; set; } = 15;
}
