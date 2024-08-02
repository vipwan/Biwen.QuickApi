namespace Biwen.QuickApi.Auditing;

/// <summary>
/// 提供 <typeparamref name="T"/> 的审计代理服务工厂,构造的代理会自动审计
/// </summary>
public class AuditProxyFactory<T>(IServiceScopeFactory serviceScopeFactory) where T : class
{
    public T Create(T impl)
    {
        ArgumentNullException.ThrowIfNull(impl);
        return AuditProxy<T>.Create(impl, serviceScopeFactory.CreateAsyncScope().ServiceProvider);
    }

    public T Create()
    {
        var impl = serviceScopeFactory.CreateAsyncScope().ServiceProvider.GetRequiredService<T>();
        return Create(impl);
    }
}
