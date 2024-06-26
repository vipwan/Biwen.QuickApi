using Biwen.QuickApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biwen.QuickApi.Storage.AliyunOss
{
    [SuppressType]
    public static class ServiceRegistration
    {
        /// <summary>
        /// 添加阿里云OSS存储
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key"></param>
        /// <param name="connectionString"></param>
        public static void AddKeyedAliyunOssStorage(this IServiceCollection services, string key, string connectionString)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

            //services.AddOptions<AliyunOssOptions>().Configure(x =>
            //{
            //    x.ConnectionString = connectionString;
            //});

            services.AddKeyedSingleton<IFileStorage>(key, (sp, _) =>
            {
                return new AliyunOssStorage(sp, connectionString);
            });

            //add factory
            services.TryAddSingleton<FileStorageFactory>();
        }
    }
}