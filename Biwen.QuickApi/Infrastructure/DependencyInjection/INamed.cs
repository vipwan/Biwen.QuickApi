namespace Biwen.QuickApi.Infrastructure.DependencyInjection;

/// <summary>
/// 针对同质服务的键名区分 <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INamed<T>
{
    /// <summary>
    /// 针对同质化服务的键名区分
    /// </summary>
    T KeyedName { get; }
}
