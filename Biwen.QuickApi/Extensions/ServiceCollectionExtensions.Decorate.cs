// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:43:25 ServiceCollectionExtensions.Decorate.cs

namespace Biwen.QuickApi;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 装饰服务,<typeparamref name="TImpl"/>必须有一个构造函数接受<typeparamref name="TService"/>类型的参数
    /// </summary>
    /// <param name="services"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static bool Decorate<TService, TImpl>(this IServiceCollection services, params object[] parameters)
    {
        var existingService = services.SingleOrDefault(s => s.ServiceType == typeof(TService));
        if (existingService is null)
            throw new ArgumentException($"No service of type {typeof(TService).Name} found.");

        ServiceDescriptor? newService;

        if (existingService.ImplementationType is not null)
        {
            newService = new ServiceDescriptor(existingService.ServiceType,
                sp =>
                {
                    TService inner =
                        (TService)ActivatorUtilities.CreateInstance(sp, existingService.ImplementationType);

                    if (inner is null)
                        throw new Exception(
                            $"Unable to instantiate decorated type via implementation type {existingService.ImplementationType.Name}.");

                    var parameters2 = new object[parameters.Length + 1];
                    Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                    parameters2[0] = inner;

                    return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2)!;
                },
                existingService.Lifetime);
        }
        else if (existingService.ImplementationInstance is not null)
        {
            newService = new ServiceDescriptor(existingService.ServiceType,
                sp =>
                {
                    TService inner = (TService)existingService.ImplementationInstance;
                    return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
                },
                existingService.Lifetime);
        }
        else if (existingService.ImplementationFactory is not null)
        {
            newService = new ServiceDescriptor(existingService.ServiceType,
                sp =>
                {
                    TService inner = (TService)existingService.ImplementationFactory(sp);
                    if (inner is null)
                        throw new Exception(
                            "Unable to instantiate decorated type via implementation factory.");

                    return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters)!;
                },
                existingService.Lifetime);
        }
        else
        {
            throw new Exception(
                "Unable to instantiate decorated type.");
        }

        services.Remove(existingService);
        services.Add(newService);

        return true;
    }

}
