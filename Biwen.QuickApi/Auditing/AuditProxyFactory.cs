namespace Biwen.QuickApi.Auditing;

/// <summary>
/// 提供 <typeparamref name="T"/> 的审计代理服务工厂,构造的代理会自动审计
/// </summary>
public class AuditProxyFactory<T>(IServiceScopeFactory serviceScopeFactory) where T : class
{
    public T Create(T impl)
    {
        ArgumentNullException.ThrowIfNull(impl);
        using var scope = serviceScopeFactory.CreateAsyncScope();
        return AuditProxy<T>.Create(impl, scope.ServiceProvider);
    }

    public T Create()
    {
        using var scope = serviceScopeFactory.CreateAsyncScope();
        var impl = scope.ServiceProvider.GetRequiredService<T>();
        return Create(impl);
    }
}
