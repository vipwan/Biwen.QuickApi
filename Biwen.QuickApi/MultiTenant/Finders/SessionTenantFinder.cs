using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

public class SessionTenantFinder<TInfo>(IHttpContextAccessor httpContextAccessor, ITenantInfoProvider<TInfo> tenantInfoProvider) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    /// <summary>
    /// TenantId
    /// </summary>
    internal static volatile string TenantId = "TenantId";

    public async Task<TInfo?> FindAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(tenantInfoProvider, nameof(tenantInfoProvider));
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

        var tenants = await tenantInfoProvider.GetAll();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(tenantId, StringComparison.OrdinalIgnoreCase));

    }
}
