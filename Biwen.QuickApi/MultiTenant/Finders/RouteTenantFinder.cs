
using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Biwen.QuickApi.MultiTenant.Finders;

public class RouteTenantFinder<TInfo>(IHttpContextAccessor httpContextAccessor, ITenantInfoProvider<TInfo> tenantInfoProvider) :
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
        ArgumentNullException.ThrowIfNull(tenantInfoProvider, nameof(tenantInfoProvider));
        if (httpContextAccessor.HttpContext == null)
        {
            return default;
        }

        var routeValue = httpContextAccessor.HttpContext.Request.RouteValues[RouteParameter]?.ToString();
        if (string.IsNullOrEmpty(routeValue))
        {
            return default;
        }
        var tenants = await tenantInfoProvider.GetAll();
        return tenants.FirstOrDefault(t => t.Identifier.Equals(routeValue, StringComparison.OrdinalIgnoreCase));
    }
}