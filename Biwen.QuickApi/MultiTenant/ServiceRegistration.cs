using Biwen.QuickApi.MultiTenant.Abstractions;
using Biwen.QuickApi.MultiTenant.Finders;

namespace Biwen.QuickApi.MultiTenant;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 添加租户信息提供者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    public static void AddTenantInfoProvider<T, TInfo>(this IServiceCollection services)
        where T : class, ITenantInfoProvider<TInfo>
        where TInfo : ITenantInfo
    {
        services.TryAddSingleton<ITenantInfoProvider<TInfo>, T>();
    }

    #region 租户查找器

    /// <summary>
    /// 添加自定义的租户查找器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    public static void AddTenantFinder<T, TInfo>(this IServiceCollection services)
        where T : class, ITenantFinder<TInfo>
        where TInfo : ITenantInfo
    {
        services.TryAddScoped<ITenantFinder<TInfo>, T>();
    }

    /// <summary>
    /// 添加基于路径的租户查找器
    /// </summary>
    /// <param name="services"></param>
    public static void AddBasePathTenantFinder<TInfo>(this IServiceCollection services)
        where TInfo : ITenantInfo
    {
        services.AddTenantFinder<BasePathTenantFinder<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加基于Header的租户查找器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="tenantIdHeader">默认空为:X-Tenant-Id</param>
    public static void AddHeaderTenantFinder<TInfo>(this IServiceCollection services, string? tenantIdHeader = null)
        where TInfo : ITenantInfo
    {
        if (!string.IsNullOrEmpty(tenantIdHeader))
        {
            HeaderTenantFinder<TInfo>.TenantIdHeader = tenantIdHeader;
        }
        services.AddTenantFinder<HeaderTenantFinder<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加基于路由的租户查找器,用于MVC项目
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="services"></param>
    /// <param name="routeParam">默认路由参数为__tenant__</param>
    public static void AddRouteTenantFinder<TInfo>(this IServiceCollection services, string? routeParam = null)
        where TInfo : ITenantInfo
    {
        if (!string.IsNullOrEmpty(routeParam))
        {
            RouteTenantFinder<TInfo>.RouteParameter = routeParam;
        }
        services.AddTenantFinder<RouteTenantFinder<TInfo>, TInfo>();
    }

    #endregion
}