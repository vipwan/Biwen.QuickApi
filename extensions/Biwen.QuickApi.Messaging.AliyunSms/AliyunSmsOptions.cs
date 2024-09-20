// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:04 AliyunSmsOptions.cs

namespace Biwen.QuickApi.Messaging.AliyunSms;

/// <summary>
/// 阿里云短信配置
/// </summary>
public class AliyunSmsOptions
{
    /// <summary>
    /// Secret
    /// </summary>
    public string AccessKeySecret { get; set; } = default!;
    /// <summary>
    /// AccessKeyId
    /// </summary>
    public string AccessKeyId { get; set; } = default!;
    /// <summary>
    /// Endpoint
    /// </summary>
    public string EndPoint { get; set; } = default!;

}
