namespace Biwen.QuickApi.Auditing;

/// <summary>
/// 提供 <typeparamref name="T"/> 的审计代理服务工厂,构造的代理会自动审计
/// </summary>
public class AuditProxyFactory<T> where T : class
{
    private readonly IServiceProvider _serviceProvider;

    public AuditProxyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create(T impl)
    {
        ArgumentNullException.ThrowIfNull(impl);
        return AuditProxy<T>.Create(impl, _serviceProvider);
    }

    public T Create()
    {
        var impl = _serviceProvider.GetRequiredService<T>();
        return Create(impl);
    }

}
