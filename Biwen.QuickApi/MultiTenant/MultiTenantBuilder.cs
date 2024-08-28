using Biwen.QuickApi.MultiTenant.Abstractions;
using Biwen.QuickApi.MultiTenant.Finders;

namespace Biwen.QuickApi.MultiTenant;

/// <summary>
/// 多租户构建器
/// </summary>
/// <typeparam name="TInfo"></typeparam>
public class MultiTenantBuilder<TInfo> where TInfo : class, ITenantInfo
{
    public MultiTenantBuilder(IServiceCollection services)
    {
        Services = services;
    }
    public IServiceCollection Services { get; }

    /// <summary>
    /// 添加租户信息提供者
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddTenantInfoProvider<TProvider>()
        where TProvider : class, ITenantInfoProvider<TInfo>
    {
        Services.TryAddSingleton<ITenantInfoProvider<TInfo>, TProvider>();
        return this;
    }

    /// <summary>
    /// 添加通过配置文件提供租户信息
    /// </summary>
    /// <param name="configSection">不填则默认:BiwenQuickApi:MultiTenants</param>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddConfigurationInfoProvider(string? configSection = null)
    {
        Services.AddConfigurationInfoProvider<TInfo>(configSection);
        return this;
    }

    /// <summary>
    /// 添加基于内存提供租户信息
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddMemoryInfoProvider(Func<IList<TInfo>?>? func)
    {
        Services.AddMemoryInfoProvider(func);
        return this;
    }

    /// <summary>
    /// 添加租户查找器
    /// </summary>
    /// <typeparam name="TFinder"></typeparam>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddTenantFinder<TFinder>()
        where TFinder : class, ITenantFinder<TInfo>
    {
        Services.TryAddScoped<ITenantFinder<TInfo>, TFinder>();
        return this;
    }

    /// <summary>
    /// 基于路径的租户查找器
    /// </summary>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddBasePathTenantFinder()
    {
        Services.AddTenantFinder<BasePathFinder<TInfo>, TInfo>();
        return this;
    }

    /// <summary>
    /// 基于Header的租户查找器
    /// </summary>
    /// <param name="tenantIdHeader"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddHeaderTenantFinder(string? tenantIdHeader = null)
    {
        if (!string.IsNullOrEmpty(tenantIdHeader))
        {
            HeaderFinder<TInfo>.TenantIdHeader = tenantIdHeader;
        }
        Services.AddTenantFinder<HeaderFinder<TInfo>, TInfo>();
        return this;
    }

    /// <summary>
    /// 基于Host的租户查找器
    /// </summary>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddHostTenantFinder()
    {
        Services.AddTenantFinder<HostFinder<TInfo>, TInfo>();
        return this;
    }

    /// <summary>
    /// 基于Session的租户查找器
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddSessionTenantFinder(string? sessionId = null)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            SessionFinder<TInfo>.TenantId = sessionId;
        }
        Services.AddTenantFinder<SessionFinder<TInfo>, TInfo>();
        return this;
    }

    /// <summary>
    /// 基于路由的租户查找器
    /// </summary>
    /// <param name="routeParameter"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TInfo> AddRouteTenantFinder(string? routeParameter = null)
    {
        if (!string.IsNullOrEmpty(routeParameter))
        {
            RouteFinder<TInfo>.RouteParameter = routeParameter;
        }
        Services.AddTenantFinder<RouteFinder<TInfo>, TInfo>();
        return this;
    }

}
