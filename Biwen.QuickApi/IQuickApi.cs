using Microsoft.AspNetCore.Builder;

namespace Biwen.QuickApi
{

    /// <summary>
    /// 标记接口
    /// </summary>
    internal interface IQuickApi<Req, Rsp>
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

}