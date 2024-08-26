using Biwen.QuickApi.MultiTenant.Abstractions;
using Biwen.QuickApi.MultiTenant.Finders;

namespace Biwen.QuickApi.MultiTenant;

/// <summary>
/// 多租户构建器
/// </summary>
/// <typeparam name="TTenantInfo"></typeparam>
public class MultiTenantBuilder<TTenantInfo> where TTenantInfo : class, ITenantInfo
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
    public MultiTenantBuilder<TTenantInfo> AddTenantInfoProvider<TProvider>()
        where TProvider : class, ITenantInfoProvider<TTenantInfo>
    {
        Services.TryAddSingleton<ITenantInfoProvider<TTenantInfo>, TProvider>();
        return this;
    }

    /// <summary>
    /// 添加租户查找器
    /// </summary>
    /// <typeparam name="TFinder"></typeparam>
    /// <returns></returns>
    public MultiTenantBuilder<TTenantInfo> AddTenantFinder<TFinder>()
        where TFinder : class, ITenantFinder<TTenantInfo>
    {
        Services.TryAddScoped<ITenantFinder<TTenantInfo>, TFinder>();
        return this;
    }

    /// <summary>
    /// 基于路径的租户查找器
    /// </summary>
    /// <returns></returns>
    public MultiTenantBuilder<TTenantInfo> AddBasePathTenantFinder()
    {
        Services.AddTenantFinder<BasePathTenantFinder<TTenantInfo>, TTenantInfo>();
        return this;
    }

    /// <summary>
    /// 基于Header的租户查找器
    /// </summary>
    /// <param name="tenantIdHeader"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TTenantInfo> AddHeaderTenantFinder(string? tenantIdHeader = null)
    {
        if (!string.IsNullOrEmpty(tenantIdHeader))
        {
            HeaderTenantFinder<TTenantInfo>.TenantIdHeader = tenantIdHeader;
        }
        Services.AddTenantFinder<HeaderTenantFinder<TTenantInfo>, TTenantInfo>();
        return this;
    }

    /// <summary>
    /// 基于Host的租户查找器
    /// </summary>
    /// <returns></returns>
    public MultiTenantBuilder<TTenantInfo> AddHostTenantFinder()
    {
        Services.AddTenantFinder<HostTenantFinder<TTenantInfo>, TTenantInfo>();
        return this;
    }

    /// <summary>
    /// 基于Session的租户查找器
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TTenantInfo> AddSessionTenantFinder(string? sessionId = null)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            SessionTenantFinder<TTenantInfo>.TenantId = sessionId;
        }
        Services.AddTenantFinder<SessionTenantFinder<TTenantInfo>, TTenantInfo>();
        return this;
    }

    /// <summary>
    /// 基于路由的租户查找器
    /// </summary>
    /// <param name="routeParameter"></param>
    /// <returns></returns>
    public MultiTenantBuilder<TTenantInfo> AddRouteTenantFinder(string? routeParameter = null)
    {
        if (!string.IsNullOrEmpty(routeParameter))
        {
            RouteTenantFinder<TTenantInfo>.RouteParameter = routeParameter;
        }
        Services.AddTenantFinder<RouteTenantFinder<TTenantInfo>, TTenantInfo>();
        return this;
    }

}
