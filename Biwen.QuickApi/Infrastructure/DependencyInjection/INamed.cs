// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:07 INamed.cs

namespace Biwen.QuickApi.Infrastructure.DependencyInjection;

/// <summary>
/// 针对同质服务的键名区分 <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INamed<T>
{
    /// <summary>
    /// 针对同质化服务的键名区分
    /// </summary>
    T KeyedName { get; }
}
