// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IProxyCache.cs

namespace Biwen.QuickApi.Caching.Abstractions;

/// <summary>
/// 提供给CachingProxy的缓存接口
/// </summary>
public interface IProxyCache
{
    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object? Get(string key);

    /// <summary>
    /// 设置缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expire"></param>
    Task SetAsync(string key, object? value, TimeSpan expire);

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key"></param>
    Task RemoveAsync(string key);

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task ExistsAsync(string key);
}
