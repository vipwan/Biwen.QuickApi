using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant;

public sealed class TenantContextAccessor<TInfo>(IHttpContextAccessor httpContextAccessor)
    where TInfo : class, ITenantInfo
{

    private TInfo? _tenantInfo;

    /// <summary>
    /// 获取上下文中的租户信息
    /// </summary>
    public TInfo? TenantInfo
    {
        get
        {
            if (_tenantInfo is not null)
            {
                return _tenantInfo;
            }

            if (httpContextAccessor?.HttpContext is null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }

            var contextService = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AsyncContextService<TInfo>>();
            contextService.TryGet(out _tenantInfo);

            return _tenantInfo;
        }
    }
}