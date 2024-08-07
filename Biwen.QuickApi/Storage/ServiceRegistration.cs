﻿using Biwen.QuickApi.Serializer;

namespace Biwen.QuickApi.Storage
{
    [SuppressType]
    public static class ServiceRegistration
    {
        /// <summary>
        /// 添加本地文件存储服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key">Keyed</param>
        /// <param name="path">物理文件夹.请注意多个存储使用key区分</param>
        public static void AddKeyedLocalFileStorage(this IServiceCollection services, string key, string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);

            services.TryAddSingleton<ISerializer, SystemTextJsonSerializer>();

            services.TryAddKeyedSingleton<IFileStorage>(key, (sp, _) =>
            {
                return new LocalFileStorage(sp, path);
            });

            //add factory
            services.TryAddSingleton<FileStorageFactory>();
        }
    }
}