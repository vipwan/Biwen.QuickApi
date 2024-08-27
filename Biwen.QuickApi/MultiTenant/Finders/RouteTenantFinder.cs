using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 基于路由的租户查找器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
/// <param name="httpContextAccessor"></param>
/// <param name="cachingProxyFactory"></param>
public class RouteTenantFinder<TInfo>(
    IHttpContextAccessor httpContextAccessor,
    CachingProxyFactory<ITenantInfoProvider<TInfo>> cachingProxyFactory) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    /// <summary>
    /// RouteParameter
    /// </summary>
    internal static volatile string RouteParameter = "__tenant__";

    public async Task<TInfo?> FindAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(cachingProxyFactory, nameof(cachingProxyFactory));
        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }

        var routeValue = httpContextAccessor.HttpContext.Request.RouteValues[RouteParameter]?.ToString();
        if (string.IsNullOrEmpty(routeValue))
        {
            return default;
        }

        var tenantInfoProvider = cachingProxyFactory.Create();

        var tenants = await tenantInfoProvider.GetAll();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(routeValue, StringComparison.OrdinalIgnoreCase));
    }
}