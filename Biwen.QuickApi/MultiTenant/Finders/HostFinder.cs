// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:48:16 HostFinder.cs

using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 基于Host的租户查找器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
public class HostFinder<TInfo>(
    IHttpContextAccessor httpContextAccessor,
    CachingProxyFactory<ITenantInfoProvider<TInfo>> cachingProxyFactory) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    public async Task<TInfo?> FindAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(cachingProxyFactory, nameof(cachingProxyFactory));
        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }

        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }

        var host = httpContextAccessor.HttpContext.Request.Host.Host;

        var tenantInfoProvider = cachingProxyFactory.Create();

        var tenants = await tenantInfoProvider.GetAllAsync();

        foreach (var tenant in tenants)
        {
            var flag = Regex.IsMatch(
                host,
                tenant.Identifier,
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

            if (flag)
            {
                return tenant;
            }
        }
        return default;
    }
}
