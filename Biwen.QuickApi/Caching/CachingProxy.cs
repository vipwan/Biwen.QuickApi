﻿namespace Biwen.QuickApi.Caching;

using System;
using System.Collections.Concurrent;
using System.Reflection;

internal class CachingProxy<T> : DispatchProxy where T : class
{
    private T? _decorated;

    private static readonly ConcurrentDictionary<string, (DateTime Exp, object? Data)> CachedDatas = new();

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

        //可以在接口定义中全局配置,也可以在实现类方法中单独配置
        if (_decorated!.GetType()
            .GetMethod(targetMethod.Name, types: args?.Select(x => x?.GetType()!)?.ToArray() ?? [])!
            .GetCustomAttribute<AutoCacheAttribute>() is not { } cacheMeta)
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

            var hasCache = CachedDatas.TryGetValue(cacheKey, out var cacheData);
            if (hasCache)
            {
                if (DateTime.Now < cacheData.Exp)
                {
                    return cacheData.Data;
                }
                CachedDatas.TryRemove(cacheKey, out _);
            }

            //如果缓存不含的情况下:
            var data = targetMethod.Invoke(_decorated, args);
            //插入缓存值
            CachedDatas.TryAdd(cacheKey,
                (DateTime.Now.Add(cacheMeta.Expiration ?? TimeSpan.FromSeconds(30 * 60)), data));

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
    /// <returns></returns>
    public static T Create(T decorated)
    {
        object proxy = Create<T, CachingProxy<T>>();
        ((CachingProxy<T>)proxy)._decorated = decorated;
        return (T)proxy;
    }

}