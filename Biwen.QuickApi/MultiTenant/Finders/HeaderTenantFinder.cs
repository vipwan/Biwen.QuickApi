using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 基于Header的租户查找器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
/// <param name="httpContextAccessor"></param>
/// <param name="cachingProxyFactory"></param>
public class HeaderTenantFinder<TInfo>(
    IHttpContextAccessor httpContextAccessor,
    CachingProxyFactory<ITenantInfoProvider<TInfo>> cachingProxyFactory) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    /// <summary>
    /// TenantIdHeader
    /// </summary>
    internal static volatile string TenantIdHeader = "X-Tenant-Id";

    public async Task<TInfo?> FindAsync()
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
