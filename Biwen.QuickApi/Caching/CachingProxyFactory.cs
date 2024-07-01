namespace Biwen.QuickApi.Caching;

public class CachingProxyFactory<T>(IServiceProvider serviceProvider)
    where T : class
{

    /// <summary>
    /// 创建代理实例
    /// </summary>
    /// <param name="impl"></param>
    /// <returns></returns>
    public T Create(T impl)
    {
        ArgumentNullException.ThrowIfNull(impl);
        return CachingProxy<T>.Create(impl);
    }

    /// <summary>
    /// 创建容器中的服务缓存代理
    /// </summary>
    /// <returns></returns>
    public T Create()
    {
        var impl = serviceProvider.GetRequiredService<T>();
        return Create(impl);
    }
}