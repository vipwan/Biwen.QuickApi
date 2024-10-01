// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 ServiceRegistration.cs

namespace Biwen.QuickApi.Events;

[SuppressType]
internal static class ServiceRegistration
{
    /// <summary>
    /// 注册事件相关服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    internal static IServiceCollection AddEvent(this IServiceCollection services)
    {
        //事件订阅者,不需要Public.
        //注册EventHanders
        foreach (var subscriberType in EventSubscribers)
        {
            //存在一个订阅者订阅多个事件的情况:
            var baseTypes = subscriberType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == InterfaceEventSubscriber).ToArray();
            foreach (var baseType in baseTypes)
            {
                services.AddScoped(baseType, subscriberType);
            }
        }
        //注册Publisher
        services.AddActivatedSingleton<Publisher>();
        return services;
    }

    static readonly Lock _lock = new();//锁
    static IEnumerable<Type> _eventSubscribers = null!;
    static readonly Type InterfaceEventSubscriber = typeof(IEventSubscriber<>);
    static bool IsToGenericInterface(Type type, Type baseInterface)
    {
        if (type == null) return false;
        if (baseInterface == null) return false;

        return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == baseInterface);
    }

    static IEnumerable<Type> EventSubscribers
    {
        get
        {
            lock (_lock)
                return _eventSubscribers ??= ASS.InAllRequiredAssemblies.Where(x =>
                !x.IsAbstract && x.IsClass && IsToGenericInterface(x, InterfaceEventSubscriber));
        }
    }
}