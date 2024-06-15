using Mapster;
using MapsterMapper;
namespace Biwen.QuickApi.Mapping
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// AddMapster 这是默认的Mapper,会扫描程序集中的配置
        /// </summary>
        /// <param name="services"></param>
        public static void AddMapsterMapper(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(ASS.AllRequiredAssemblies);
            // 配置默认全局映射（支持覆盖）
            config.Default
            .NameMatchingStrategy(NameMatchingStrategy.Flexible)
                  .PreserveReference(true);

            // 配置默认全局映射（忽略大小写敏感）
            config.Default
                  .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)
                  .PreserveReference(true);

            services.AddActivatedSingleton(_ => config);
            services.AddScoped<IMapper, ServiceMapper>();
        }
    }
}