namespace Biwen.QuickApi.Caching.ProxyCache;

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
    Task Set(string key, object? value, TimeSpan expire);

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key"></param>
    Task Remove(string key);

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task Exists(string key);
}
