using Biwen.QuickApi.MultiTenant.Abstractions;
using Biwen.QuickApi.MultiTenant.Finders;
using Biwen.QuickApi.MultiTenant.InfoProviders;

namespace Biwen.QuickApi.MultiTenant;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 添加租户信息提供者
    /// </summary>
    /// <typeparam name="TInfoProvider"></typeparam>
    /// <param name="services"></param>
    public static void AddTenantInfoProvider<TInfoProvider, TInfo>(this IServiceCollection services)
        where TInfoProvider : class, ITenantInfoProvider<TInfo>
        where TInfo : class, ITenantInfo
    {
        services.TryAddSingleton<ITenantInfoProvider<TInfo>, TInfoProvider>();
    }

    /// <summary>
    /// 添加通过配置文件提供租户信息
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="services"></param>
    /// <param name="configSection">不填则默认:BiwenQuickApi:MultiTenants</param>
    internal static void AddConfigurationInfoProvider<TInfo>(this IServiceCollection services, string? configSection = null)
        where TInfo : class, ITenantInfo
    {
        if (!string.IsNullOrEmpty(configSection))
        {
            ConfigurationInfoProvider<TInfo>.DefaultSectionName = configSection;
        }
        services.AddTenantInfoProvider<ConfigurationInfoProvider<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加基于内存提供租户信息
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="services"></param>
    /// <param name="func"></param>
    internal static void AddMemoryInfoProvider<TInfo>(this IServiceCollection services, Func<IList<TInfo>?>? func)
        where TInfo : class, ITenantInfo
    {
        MemoryInfoProvider<TInfo>._infos = func?.Invoke();
        services.AddTenantInfoProvider<MemoryInfoProvider<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加租户上下文Accessor
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="services"></param>
    internal static void AddTenantContextAccessor<TInfo>(this IServiceCollection services)
        where TInfo : class, ITenantInfo
    {
        services.AddScoped<TenantContextAccessor<TInfo>>();
    }

    #region 租户查找器

    /// <summary>
    /// 添加自定义的租户查找器
    /// </summary>
    /// <typeparam name="TFinder"></typeparam>
    /// <param name="services"></param>
    public static void AddTenantFinder<TFinder, TInfo>(this IServiceCollection services)
        where TFinder : class, ITenantFinder<TInfo>
        where TInfo : class, ITenantInfo
    {
        services.TryAddScoped<ITenantFinder<TInfo>, TFinder>();
    }

    /// <summary>
    /// 添加基于路径的租户查找器
    /// </summary>
    /// <param name="services"></param>
    public static void AddBasePathTenantFinder<TInfo>(this IServiceCollection services)
        where TInfo : class, ITenantInfo
    {
        services.AddTenantFinder<BasePathFinder<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加基于Header的租户查找器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="tenantIdHeader">默认空为:X-Tenant-Id</param>
    public static void AddHeaderTenantFinder<TInfo>(this IServiceCollection services, string? tenantIdHeader = null)
        where TInfo : class, ITenantInfo
    {
        if (!string.IsNullOrEmpty(tenantIdHeader))
        {
            HeaderFinder<TInfo>.TenantIdHeader = tenantIdHeader;
        }
        services.AddTenantFinder<HeaderFinder<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加基于路由的租户查找器,用于MVC项目
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="services"></param>
    /// <param name="routeParam">默认路由参数为__tenant__</param>
    public static void AddRouteTenantFinder<TInfo>(this IServiceCollection services, string? routeParam = null)
        where TInfo : class, ITenantInfo
    {
        if (!string.IsNullOrEmpty(routeParam))
        {
            RouteFinder<TInfo>.RouteParameter = routeParam;
        }
        services.AddTenantFinder<RouteFinder<TInfo>, TInfo>();
    }

    /// <summary>
    /// 添加基于Session的租户查找器
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    /// <param name="services"></param>
    /// <param name="sessionId"></param>
    public static void AddSessionTenantFinder<TInfo>(this IServiceCollection services, string? sessionId = null)
        where TInfo : class, ITenantInfo
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            SessionFinder<TInfo>.TenantId = sessionId;
        }
        services.AddTenantFinder<SessionFinder<TInfo>, TInfo>();
    }

    #endregion

    #region 扩展MultiTenantBuilder

    public static MultiTenantBuilder<TInfo> AddMultiTenant<TInfo>(this IServiceCollection services)
        where TInfo : class, ITenantInfo
    {
        return services.AddMultiTenant<TInfo>(o =>
        {
            o.Enabled = true;
            o.TenantInfoType = typeof(TInfo);
        });
    }

    public static MultiTenantBuilder<TInfo> AddMultiTenant<TInfo>(this IServiceCollection services,
    Action<MultiTenantOptions> config)
    where TInfo : class, ITenantInfo
    {
        //添加配置
        services.Configure(config);

        //添加租户上下文Accessor
        services.AddTenantContextAccessor<TInfo>();

        return new MultiTenantBuilder<TInfo>(services);
    }

    #endregion
}