// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:56:48 ServiceRegistration.cs

using System.Diagnostics.CodeAnalysis;

namespace Biwen.QuickApi.Messaging.Sms;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add SmsSender,如果只有一个实现的情况下
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    public static void AddSmsSender<T>(this IServiceCollection services) where T : class, ISmsSender
    {
        services.TryAddScoped<ISmsSender, T>();
    }

    /// <summary>
    /// 添加空的短信发送器
    /// </summary>
    /// <param name="services"></param>
    public static void AddNullSmsSender(this IServiceCollection services)
    {
        AddSmsSender<NullSmsSender>(services);
    }

    /// <summary>
    /// 当存在多个实现时，需要指定key注入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="key"></param>
    public static void AddKeyedSmsSender<T>(this IServiceCollection services, [NotNull] string key) where T : class, ISmsSender
    {
        services.TryAddKeyedScoped<ISmsSender, T>(key);
    }


    /// <summary>
    /// 返回指定键的短信发送器
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static ISmsSender? GetKeyedSmsSender(this IServiceProvider serviceProvider, string key)
    {
        return serviceProvider.GetServices<ISmsSender>().FirstOrDefault(x => x.KeyedName == key);
    }
}