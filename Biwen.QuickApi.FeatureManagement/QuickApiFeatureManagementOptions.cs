// Licensed to the Biwen.QuickApi.FeatureManagement under one or more agreements.
// The Biwen.QuickApi.FeatureManagement licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.FeatureManagement Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:58:59 QuickApiFeatureManagementOptions.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.FeatureManagement;

/// <summary>
/// QuickApiFeatureManagement配置项
/// </summary>
public class QuickApiFeatureManagementOptions
{
    /// <summary>
    /// 失败返回的状态码,默认:404
    /// </summary>
    public int StatusCode { get; set; } = StatusCodes.Status404NotFound;

    /// <summary>
    /// 失败时的处理.
    /// </summary>
    public Action<HttpContext>? OnErrorAsync { get; set; } = null;

}
