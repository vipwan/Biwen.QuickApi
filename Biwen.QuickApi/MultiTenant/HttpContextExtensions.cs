using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.MultiTenant;

[SuppressType]
public static class HttpContextExtensions
{
    /// <summary>
    /// 获取租户信息
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TInfo? GetTenantInfo<TInfo>(this HttpContext context)
        where TInfo : class, ITenantInfo
    {
        var tenantContextService = context.RequestServices.GetRequiredService<AsyncContextService<TInfo>>();
        return tenantContextService.Get();
    }

    /// <summary>
    /// 设置租户信息
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="context"></param>
    /// <param name="info"></param>
    public static void SetTenantInfo<TInfo>(this HttpContext context, TInfo info)
        where TInfo : class, ITenantInfo
    {
        var tenantContextService = context.RequestServices.GetRequiredService<AsyncContextService<TInfo>>();
        tenantContextService.Set(info);
    }
}