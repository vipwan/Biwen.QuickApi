// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:46 HttpContextExtensions.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant;

[SuppressType]
public static class HttpContextExtensions
{
    /// <summary>
    /// 获取租户信息
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TInfo? GetTenantInfo<TInfo>(this HttpContext context)
        where TInfo : class, ITenantInfo
    {
        var tenantContextService = context.RequestServices.GetRequiredService<TenantContextAccessor<TInfo>>();
        return tenantContextService.TenantInfo;
    }
}