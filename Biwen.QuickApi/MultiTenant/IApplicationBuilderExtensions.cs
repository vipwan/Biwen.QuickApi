using Biwen.QuickApi.MultiTenant.Internal;
using Microsoft.AspNetCore.Builder;

namespace Biwen.QuickApi.MultiTenant;

[SuppressType]
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// 使用多租户中间件,系统会自动获取租户信息
    /// </summary>
    public static IApplicationBuilder UseMultiTenant<TInfo>(this IApplicationBuilder builder)
        where TInfo : class, ITenantInfo
        => builder.UseMiddleware<MultiTenantMiddleware<TInfo>>();
}
