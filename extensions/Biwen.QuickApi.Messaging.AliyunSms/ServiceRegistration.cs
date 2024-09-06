// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:17 ServiceRegistration.cs

using Biwen.QuickApi.Infrastructure;
using Biwen.QuickApi.Messaging.Sms;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Messaging.AliyunSms;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// Add AliyunSmsSender
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    public static void AddAliyunSmsSender(this IServiceCollection services, Action<AliyunSmsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        services.Configure(options);
        services.AddSmsSender<AliyunSmsSender>();
    }
}