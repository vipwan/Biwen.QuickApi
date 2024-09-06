// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 ServiceRegistration.cs

using Biwen.QuickApi.Auditing.Abstractions;

namespace Biwen.QuickApi.Auditing;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 添加审计处理器,比如写入数据库,写入日志等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuditHandler<T>(this IServiceCollection services)
        where T : IAuditHandler
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Transient(typeof(IAuditHandler),
            typeof(T)));
        return services;
    }

    /// <summary>
    /// 移除所有审计处理器,默认自带的ConsoleAuditHandler也将被移除
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static ServiceCollection CleanAllAuditHandler(this ServiceCollection services)
    {
        var descriptors = services.Where(x => x.ServiceType == typeof(IAuditHandler));
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
        return services;
    }
}