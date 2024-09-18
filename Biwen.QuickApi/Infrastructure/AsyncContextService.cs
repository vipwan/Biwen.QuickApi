// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:58 AsyncContextService.cs

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AsyncState;

namespace Biwen.QuickApi.Infrastructure;

/// <summary>
/// 异步流Context服务
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class AsyncContextService<T> : IAsyncContext<T> where T : class
{
    public AsyncContextService(IServiceProvider serviceProvider)
    {
        _asyncState = serviceProvider.GetRequiredService<IAsyncState>()!;
        _asyncContext = serviceProvider.GetRequiredService<IAsyncContext<T>>();

        httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>()!;
        if (httpContextAccessor?.HttpContext is null)
        {
            //如果不是HttpContext请求，初始化AsyncState, 用于非HttpContext调用
            //对于HttpContext请求，异步流存放在HttpContext.Features中
            _asyncState.Initialize();
        }
    }

    private readonly IAsyncState _asyncState;
    private readonly IHttpContextAccessor? httpContextAccessor;
    private readonly IAsyncContext<T> _asyncContext;

    /// <summary>
    /// Get
    /// </summary>
    /// <returns></returns>
    public T? Get() => _asyncContext.Get();
    /// <summary>
    /// TryGet
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGet(out T? value) => _asyncContext.TryGet(out value);
    /// <summary>
    /// Set
    /// </summary>
    /// <param name="value"></param>
    public void Set(T? value) => _asyncContext.Set(value);

}