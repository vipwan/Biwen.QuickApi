// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:39 ServiceRegistration.cs

using System.Diagnostics.CodeAnalysis;

namespace Biwen.QuickApi.Messaging.Email;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add SmsSender,如果只有一个实现的情况下
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    public static void AddEmailSender<T>(this IServiceCollection services) where T : class, IEmailSender
    {
        services.TryAddScoped<IEmailSender, T>();
    }

    /// <summary>
    /// Add Keyed SmsSender,如果有多个的情况下需要指定key注入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="key"></param>
    public static void AddKeyedEmailSender<T>(this IServiceCollection services, [NotNull] string key) where T : class, IEmailSender
    {
        services.TryAddKeyedScoped<IEmailSender, T>(key);
    }

    /// <summary>
    /// 添加空的邮件发送器
    /// </summary>
    /// <param name="services"></param>
    public static void AddNullEmailSender(this IServiceCollection services)
    {
        AddEmailSender<NullEmailSender>(services);
    }

    /// <summary>
    /// 返回指定键的短信发送器
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IEmailSender? GetKeyedEmailSender(this IServiceProvider serviceProvider, string key)
    {
        return serviceProvider.GetServices<IEmailSender>().FirstOrDefault(x => x.KeyedName == key);
    }
}