using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Abstractions
{
    using Biwen.QuickApi.Events;

    /// <summary>
    /// 标记接口
    /// </summary>
#pragma warning disable GEN031 // 使用[AutoGen]自动生成
    internal interface IQuickApi<Req, Rsp> : IHandlerBuilder, IQuickApiMiddlewareHandler, IAntiforgeryApi, IPublisher
    {
        ValueTask<Rsp> ExecuteAsync(Req request);
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
        /// <returns></returns>
        Task BeforeAsync(HttpContext context);
        /// <summary>
        /// 请求QuickApi后的操作
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task AfterAsync(HttpContext context);
    }

    /// <summary>
    /// 防伪令牌检测
    /// </summary>
    public interface IAntiforgeryApi
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
        /// <param name="event">Event</param>
        /// <returns></returns>
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : class, IEvent;
    }

}