using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biwen.QuickApi.Serializer
{
    [SuppressType]
    public static class ServiceRegistration
    {
        /// <summary>
        /// Add AddSerializer,如果不指定系统内部会默认:SystemTextJsonSerializer,
        /// 如果不在容器中使用可以直接调用: DefaultSerializer.Instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSerializer<T>(this IServiceCollection services) where T : class, ISerializer
        {
            services.TryAddSingleton<ISerializer, T>();
            return services;
        }

        /// <summary>
        /// Add SystemTextJsonSerializer
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSystemTextJsonSerializer(this IServiceCollection services)
        {
            services.AddSerializer<SystemTextJsonSerializer>();
            return services;
        }
    }
}