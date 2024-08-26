using Biwen.QuickApi.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Biwen.QuickApi.MultiTenant.Finders;

/// <summary>
/// 基于Host的租户查找器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
public class HostTenantFinder<TInfo>(IHttpContextAccessor httpContextAccessor, ITenantInfoProvider<TInfo> tenantInfoProvider) :
    ITenantFinder<TInfo>
    where TInfo : ITenantInfo
{
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

        var host = httpContextAccessor.HttpContext.Request.Host.Host;
        var tenants = await tenantInfoProvider.GetAll();

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
