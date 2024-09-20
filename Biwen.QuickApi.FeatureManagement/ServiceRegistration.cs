// Licensed to the Biwen.QuickApi.FeatureManagement under one or more agreements.
// The Biwen.QuickApi.FeatureManagement licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.FeatureManagement Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:59:03 ServiceRegistration.cs

using Biwen.QuickApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.FeatureManagement;


[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 自定义配置QuickApiFeatureManagementOptions
    /// </summary>
    /// <param name="services"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureQuickApiFeatureManagementOptions(this IServiceCollection services, Action<QuickApiFeatureManagementOptions>? action)
    {
        services
            .Configure<QuickApiFeatureManagementOptions>(static _ => { })
            .Configure<QuickApiFeatureManagementOptions>(o => action?.Invoke(o));
        return services;
    }
}