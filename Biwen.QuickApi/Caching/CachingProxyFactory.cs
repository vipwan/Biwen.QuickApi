using Biwen.QuickApi.Caching.ProxyCache;

namespace Biwen.QuickApi.Caching;

/// <summary>
/// 提供 <typeparamref name="T"/> 的缓存代理服务工厂,构造的代理会自动缓存方法的返回值
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="serviceProvider"></param>
public class CachingProxyFactory<T>(IServiceProvider serviceProvider)
    where T : class
{
    /// <summary>
    /// 根据实例创建代理
    /// </summary>
    /// <param name="impl"></param>
    /// <param name="proxyCache"></param>
    /// <returns></returns>
    public T Create(T impl, IProxyCache proxyCache)
    {
        ArgumentNullException.ThrowIfNull(impl);
        return CachingProxy<T>.Create(impl, proxyCache);
    }

    /// <summary>
    /// 从DI容器中创建代理
    /// </summary>
    /// <returns></returns>
    public T Create()
    {
        var impl = serviceProvider.GetRequiredService<T>();
        var proxyCache = serviceProvider.GetRequiredService<IProxyCache>();
        return Create(impl, proxyCache);
    }
}