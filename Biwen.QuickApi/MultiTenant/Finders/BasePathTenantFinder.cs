using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 通过路径查找租户,默认取第一个路径作为租户标识
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="tenantInfoProvider"></param>
public class BasePathTenantFinder<TInfo>(IHttpContextAccessor httpContextAccessor, ITenantInfoProvider<TInfo> tenantInfoProvider) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
    public virtual async Task<TInfo?> FindAsync()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(tenantInfoProvider, nameof(tenantInfoProvider));

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

        var tenants = await tenantInfoProvider.GetAll();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
    }
}
