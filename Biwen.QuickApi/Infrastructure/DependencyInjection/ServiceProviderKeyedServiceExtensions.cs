// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:18 ServiceProviderKeyedServiceExtensions.cs

namespace Biwen.QuickApi.Infrastructure.DependencyInjection;

internal static class ServiceProviderKeyedServiceExtensions
{
    public static object? GetKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
    {

        ArgumentNullException.ThrowIfNull(provider);

        if (provider is IKeyedServiceProvider keyedServiceProvider)
        {
            return keyedServiceProvider.GetKeyedService(serviceType, serviceKey);
        }

        throw new InvalidOperationException("This service provider doesn't support keyed services.");
    }

}
