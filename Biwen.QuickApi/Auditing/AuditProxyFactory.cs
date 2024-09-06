// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 AuditProxyFactory.cs

namespace Biwen.QuickApi.Auditing;

/// <summary>
/// 提供 <typeparamref name="T"/> 的审计代理服务工厂,构造的代理会自动审计
/// </summary>
public class AuditProxyFactory<T>(IServiceScopeFactory serviceScopeFactory) where T : class
{
    public T Create(T impl)
    {
        ArgumentNullException.ThrowIfNull(impl);
        using var scope = serviceScopeFactory.CreateAsyncScope();
        return AuditProxy<T>.Create(impl, scope.ServiceProvider);
    }

    public T Create()
    {
        using var scope = serviceScopeFactory.CreateAsyncScope();
        var impl = scope.ServiceProvider.GetRequiredService<T>();
        return Create(impl);
    }
}
