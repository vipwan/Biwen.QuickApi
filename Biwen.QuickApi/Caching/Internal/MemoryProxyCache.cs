// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 MemoryProxyCache.cs

using Biwen.QuickApi.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Biwen.QuickApi.Caching.Internal;

/// <summary>
/// 默认使用内存缓存
/// </summary>
internal sealed class MemoryProxyCache(IMemoryCache memoryCache) : IProxyCache
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

    public object? Get(string key)
    {
        return _memoryCache.Get(key);
    }

    public Task SetAsync(string key, object? value, TimeSpan expire)
    {
        _memoryCache.Set(key, value, expire);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task ExistsAsync(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }
}