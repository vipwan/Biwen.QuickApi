using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

public class HeaderTenantFinder<TInfo>(IHttpContextAccessor httpContextAccessor, ITenantInfoProvider<TInfo> tenantInfoProvider) :
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
        ArgumentNullException.ThrowIfNull(tenantInfoProvider, nameof(tenantInfoProvider));
        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }
        var header = httpContextAccessor.HttpContext.Request.Headers[TenantIdHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(header))
        {
            return default;
        }

        var tenants = await tenantInfoProvider.GetAll();
        return tenants.FirstOrDefault(t => t.Id.Equals(header, StringComparison.OrdinalIgnoreCase));
    }
}
