// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:31 ServiceRegistration.cs

using Biwen.QuickApi.Serializer;

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