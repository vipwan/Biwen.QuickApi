using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{

    /// <summary>
    /// 标记接口
    /// </summary>
    internal interface IQuickApi<Req, Rsp> : IHandlerBuilder, IQuickApiMiddlewareHandler, IAntiforgeryApi
    {
        Task<Rsp> ExecuteAsync(Req request);
    }

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
        Task InvokeBeforeAsync(HttpContext context);
        /// <summary>
        /// 请求QuickApi后的操作
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task InvokeAfterAsync(HttpContext context);
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
}