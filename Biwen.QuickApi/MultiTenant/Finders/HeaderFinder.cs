// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:12 HeaderFinder.cs

using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 基于Header的租户查找器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
/// <param name="httpContextAccessor"></param>
/// <param name="cachingProxyFactory"></param>
public class HeaderFinder<TInfo>(
    IHttpContextAccessor httpContextAccessor,
    CachingProxyFactory<ITenantInfoProvider<TInfo>> cachingProxyFactory) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    /// <summary>
    /// TenantIdHeader
    /// </summary>
    internal static volatile string TenantIdHeader = "X-Tenant-Id";

    public virtual async Task<TInfo?> FindAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(cachingProxyFactory, nameof(cachingProxyFactory));
        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }
        var header = httpContextAccessor.HttpContext.Request.Headers[TenantIdHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(header))
        {
            return default;
        }

        var tenantInfoProvider = cachingProxyFactory.Create();

        var tenants = await tenantInfoProvider.GetAllAsync();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(header, StringComparison.OrdinalIgnoreCase));
    }
}
