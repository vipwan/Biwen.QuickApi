namespace Biwen.QuickApi.Caching;

/// <summary>
/// 提供 <typeparamref name="T"/> 的缓存代理服务工厂,构造的代理会自动缓存方法的返回值
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="serviceProvider"></param>
public class CachingProxyFactory<T>(IServiceProvider serviceProvider) where T : class
{
    /// <summary>
    /// 根据实例创建代理
    /// </summary>
    /// <param name="impl"></param>
    /// <returns></returns>
    public T Create(T impl)
    {
        ArgumentNullException.ThrowIfNull(impl);
        return CachingProxy<T>.Create(impl);
    }

    /// <summary>
    /// 从DI容器中创建代理
    /// </summary>
    /// <returns></returns>
    public T Create()
    {
        var impl = serviceProvider.GetRequiredService<T>();
        return Create(impl);
    }
}