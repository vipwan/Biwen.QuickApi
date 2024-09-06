// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:49 IApplicationBuilderExtensions.cs

using Biwen.QuickApi.MultiTenant.Internal;
using Microsoft.AspNetCore.Builder;

namespace Biwen.QuickApi.MultiTenant;

[SuppressType]
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// 使用多租户中间件,系统会自动获取租户信息
    /// </summary>
    public static IApplicationBuilder UseMultiTenant<TInfo>(this IApplicationBuilder builder)
        where TInfo : class, ITenantInfo
        => builder.UseMiddleware<MultiTenantMiddleware<TInfo>>();
}
