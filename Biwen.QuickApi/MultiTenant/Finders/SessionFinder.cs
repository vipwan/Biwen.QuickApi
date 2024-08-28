using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 基于Session的租户查找器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
/// <param name="httpContextAccessor"></param>
/// <param name="cachingProxyFactory"></param>
public class SessionFinder<TInfo>(
    IHttpContextAccessor httpContextAccessor,
    CachingProxyFactory<ITenantInfoProvider<TInfo>> cachingProxyFactory) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    /// <summary>
    /// TenantId
    /// </summary>
    internal static volatile string TenantId = "TenantId";

    public virtual async Task<TInfo?> FindAsync()
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

        var session = httpContextAccessor.HttpContext.Session;
        if (session == null)
        {
            return default;
        }
        var tenantId = session.GetString(TenantId);
        if (string.IsNullOrEmpty(tenantId))
        {
            return default;
        }

        var tenantInfoProvider = cachingProxyFactory.Create();

        var tenants = await tenantInfoProvider.GetAllAsync();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(tenantId, StringComparison.OrdinalIgnoreCase));

    }
}
