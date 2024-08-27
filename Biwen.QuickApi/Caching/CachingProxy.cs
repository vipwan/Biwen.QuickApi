namespace Biwen.QuickApi.Caching;

using Biwen.QuickApi.Caching.Abstractions;
using System;
using System.Reflection;

internal class CachingProxy<T> : DispatchProxy where T : class
{
    private T? _decorated;
    private IProxyCache _proxyCache { get; set; } = null!;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        //如果方法没有返回值，直接调用原始方法
        if (targetMethod.ReturnType == typeof(void))
        {
            return targetMethod.Invoke(_decorated, args);
        }
        //如果是异步方法，直接调用原始方法
        if (targetMethod.ReturnType == typeof(Task) || targetMethod.ReturnType == typeof(ValueTask))
        {
            return targetMethod.Invoke(_decorated, args);
        }

        AutoCacheAttribute? cacheMeta = null;

        //装饰器上的配置
        cacheMeta = _decorated!.GetType()
            .GetMethod(targetMethod.Name, types: args?.Select(x => x?.GetType()!)?.ToArray() ?? [])!
            .GetCustomAttribute<AutoCacheAttribute>();

        //原始方法上的配置
        cacheMeta ??= targetMethod.GetCustomAttribute<AutoCacheAttribute>();

        //如果都没有配置 或者明确禁用 ，直接调用原始方法
        if (cacheMeta is null || (!cacheMeta.IsEnabled))
        {
            return targetMethod.Invoke(_decorated, args);
        }

        try
        {
            var cacheKey = $"{_decorated!.GetType().FullName}.{targetMethod.Name}.";
            //串联参数:
            if (args != null)
            {
                cacheKey += string.Join('&', args);
            }

            if (_proxyCache.Get(cacheKey) is { } cache)
            {
                return cache;
            }

            //如果缓存不含的情况下:
            var data = targetMethod.Invoke(_decorated, args);
            _proxyCache.Set(cacheKey, data, cacheMeta.Expiration ?? TimeSpan.FromSeconds(1800));

            return data;

        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie; // 将原始异常抛出
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// 构造代理
    /// </summary>
    /// <param name="decorated"></param>
    /// <param name="proxyCache"></param>
    /// <returns></returns>
    public static T Create(T decorated, IProxyCache proxyCache)
    {
        object proxy = Create<T, CachingProxy<T>>();
        ((CachingProxy<T>)proxy)._decorated = decorated;
        ((CachingProxy<T>)proxy)._proxyCache = proxyCache;
        return (T)proxy;
    }
}