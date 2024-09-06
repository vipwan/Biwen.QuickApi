// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:12 FileStorageFactory.cs

namespace Biwen.QuickApi.Storage
{
    /// <summary>
    /// FileStorage Factory
    /// </summary>
    public class FileStorageFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FileStorageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFileStorage Create(string key)
        {
            return _serviceProvider.GetRequiredKeyedService<IFileStorage>(key);
        }
    }
}