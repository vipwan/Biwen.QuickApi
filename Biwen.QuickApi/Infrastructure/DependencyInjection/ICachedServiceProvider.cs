// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:02 ICachedServiceProvider.cs

namespace Biwen.QuickApi.Infrastructure.DependencyInjection;

/// <summary>
/// 提供ServiceProvider的缓存DI,能保证在同一个作用域获取到的服务是同一个实例
/// </summary>
public interface ICachedServiceProvider : IKeyedServiceProvider
{
    /// <summary>
    /// 获取服务,保证在同一个作用域获取到的服务是同一个实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    T GetService<T>(T defaultValue);
    /// <summary>
    /// 获取服务,保证在同一个作用域获取到的服务是同一个实例
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    object GetService(Type serviceType, object defaultValue);
    /// <summary>
    /// 获取服务,保证在同一个作用域获取到的服务是同一个实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    /// <returns></returns>
    T GetService<T>(Func<IServiceProvider, object> factory);
    /// <summary>
    /// 获取服务,保证在同一个作用域获取到的服务是同一个实例
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    object GetService(Type serviceType, Func<IServiceProvider, object> factory);

}
