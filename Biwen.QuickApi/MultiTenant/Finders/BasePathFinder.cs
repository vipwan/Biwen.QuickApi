// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:08 BasePathFinder.cs

using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 通过路径查找租户,默认取第一个路径作为租户标识
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="cachingProxyFactory"></param>
public class BasePathFinder<TInfo>(
    IHttpContextAccessor httpContextAccessor,
    CachingProxyFactory<ITenantInfoProvider<TInfo>> cachingProxyFactory) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    public virtual async Task<TInfo?> FindAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(cachingProxyFactory, nameof(cachingProxyFactory));

        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }

        var path = httpContextAccessor.HttpContext.Request.Path;

        var pathSegments =
            path.Value?.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);

        if (pathSegments is null || pathSegments.Length == 0)
            return default;

        //取到第一个路径作为租户标识
        string identifier = pathSegments[0];

        var tenantInfoProvider = cachingProxyFactory.Create();

        var tenants = await tenantInfoProvider.GetAllAsync();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
    }
}
