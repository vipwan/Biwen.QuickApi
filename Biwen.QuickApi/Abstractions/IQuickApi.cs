// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IQuickApi.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Abstractions
{
    using Biwen.QuickApi.Events;

    /// <summary>
    /// 标记接口
    /// </summary>
#pragma warning disable GEN031 // 使用[AutoGen]自动生成
    internal interface IQuickApi<Req, Rsp> : IHandlerBuilder, IQuickApiMiddlewareHandler, IAntiforgeryApi, IPublisher, ICancel
    {
        ValueTask<Rsp> ExecuteAsync(Req request, CancellationToken cancellationToken = default);
    }
#pragma warning restore GEN031 // 使用[AutoGen]自动生成

    /// <summary>
    /// HandlerBuilder
    /// </summary>
    internal interface IHandlerBuilder
    {
        /// <summary>
        /// 提供minimal扩展,可以扩充缓存,日志,鉴权等功能,..
        /// 注意OpenApi的Produces<>QuickApi已经默认实现.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder);
    }

    /// <summary>
    /// 中间件支持
    /// </summary>
    internal interface IQuickApiMiddlewareHandler
    {
        /// <summary>
        /// 请求QuickApi前的操作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task BeforeAsync(HttpContext context, CancellationToken cancellationToken = default);
        /// <summary>
        /// 请求QuickApi后的操作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AfterAsync(HttpContext context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 防伪令牌检测
    /// </summary>
    internal interface IAntiforgeryApi
    {
        /// <summary>
        /// 是否启动防伪令牌检测
        /// </summary>
        bool IsAntiforgeryEnabled { get; }
    }


    internal interface IPublisher
    {
        /// <summary>
        /// Event Publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <param name="event">Event</param>
        /// <returns></returns>
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : class, IEvent;
    }

    internal interface ICancel
    {
        /// <summary>
        /// 中断请求
        /// </summary>
        /// <returns></returns>
        Task CancelAsync();
    }
}