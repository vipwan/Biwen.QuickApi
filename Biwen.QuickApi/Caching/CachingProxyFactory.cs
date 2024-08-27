using Biwen.QuickApi.Caching.Abstractions;

namespace Biwen.QuickApi.Caching;

/// <summary>
/// 提供 <typeparamref name="T"/> 的缓存代理服务工厂,构造的代理会自动缓存方法的返回值
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="serviceScopeFactory"></param>
public class CachingProxyFactory<T>(IServiceScopeFactory serviceScopeFactory)
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
    /// <param name="keyed">针对Keyed的服务实现别名,空表示非keyed</param>
    /// <returns></returns>
    public T Create(string? keyed = default)
    {
        using var scope = serviceScopeFactory.CreateAsyncScope();

        var impl = string.IsNullOrEmpty(keyed) ?
            scope.ServiceProvider.GetRequiredService<T>() :
            scope.ServiceProvider.GetRequiredKeyedService<T>(keyed);

        var proxyCache = scope.ServiceProvider.GetRequiredService<IProxyCache>();
        return Create(impl, proxyCache);

    }
}