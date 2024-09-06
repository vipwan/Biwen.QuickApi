// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:53:57 ServiceRegistration.cs

namespace Biwen.QuickApi.Serializer;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add AddSerializer,如果不指定系统内部会默认:SystemTextJsonSerializer,
    /// 如果不在容器中使用可以直接调用: DefaultSerializer.Instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSerializer<T>(this IServiceCollection services) where T : class, ISerializer
    {
        services.TryAddSingleton<ISerializer, T>();
        return services;
    }

    /// <summary>
    /// Add SystemTextJsonSerializer
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSystemTextJsonSerializer(this IServiceCollection services)
    {
        services.AddSerializer<SystemTextJsonSerializer>();
        return services;
    }
}