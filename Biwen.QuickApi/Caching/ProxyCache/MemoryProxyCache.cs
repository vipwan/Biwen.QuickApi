using Microsoft.Extensions.Caching.Memory;

namespace Biwen.QuickApi.Caching.ProxyCache;

/// <summary>
/// 默认使用内存缓存
/// </summary>
internal sealed class MemoryProxyCache : IProxyCache
{
    private readonly IMemoryCache _memoryCache;

    public MemoryProxyCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public object? Get(string key)
    {
        return _memoryCache.Get(key);
    }


    public Task Set(string key, object? value, TimeSpan expire)
    {
        _memoryCache.Set(key, value, expire);
        return Task.CompletedTask;
    }

    public Task Remove(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task Exists(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }
}