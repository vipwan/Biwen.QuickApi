using Biwen.QuickApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.FeatureManagement
{

    [SuppressType]
    public static class ServiceRegistration
    {
        /// <summary>
        /// 自定义配置QuickApiFeatureManagementOptions
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureQuickApiFeatureManagementOptions(this IServiceCollection services, Action<QuickApiFeatureManagementOptions>? action)
        {
            services
                .Configure<QuickApiFeatureManagementOptions>(static _ => { })
                .Configure<QuickApiFeatureManagementOptions>(o => action?.Invoke(o));
            return services;
        }
    }
}