using Microsoft.AspNetCore.Builder;
using System.Security.Cryptography;

namespace Biwen.QuickApi
{

    /// <summary>
    ///  请求方式
    /// </summary>
    [Flags]
    public enum Verb
    {
        /// <summary>
        /// 这是默认值，如果没有指定Verb，那么默认是GET
        /// </summary>
        GET = 1,
        POST = 2,
        PUT = 4,
        DELETE = 8,
        PATCH = 16,
        HEAD = 32,
        //OPTIONS = 64,
    }

    /// <summary>
    /// Request对象来源
    /// </summary>
    [Obsolete("1.1.0版本后，不再使用,请使用IReqBinder", false)]
    public enum RequestFrom
    {
        /// <summary>
        /// 这是默认值，如果没有指定From，那么默认是FromQuery
        /// </summary>
        FromQuery,
        FromForm,
        FromBody,
        FromRoute,
        FromHead
    }

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
        /// 提供minimal扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder);
    }


    /// <summary>
    /// BaseQuickApi
    /// </summary>
    /// <typeparam name="Req">请求对象</typeparam>
    /// <typeparam name="Rsp">输出对象</typeparam>
    public abstract class BaseQuickApi<Req, Rsp> : IHandlerBuilder, IQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
    {
        /// <summary>
        /// 获取请求类型
        /// </summary>
        public Type ReqType => typeof(Req);

        /// <summary>
        /// 请求输出,注意如果需要Request对象，请使用HttpContextAccessor.HttpContext.Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<Rsp> ExecuteAsync(Req request);

        public virtual RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //todo:
            return builder;
        }
    }

    /// <summary>
    /// 有请求参数的BaseQuickApi,没有返回值
    /// </summary>
    /// <typeparam name="Req"></typeparam>
    public abstract class BaseQuickApi<Req> : BaseQuickApi<Req, EmptyResponse> where Req : BaseRequest<Req>, new()
    {
    }

    /// <summary>
    /// 没有请求参数的BaseQuickApi,没有返回值
    /// </summary>
    public abstract class BaseQuickApi : BaseQuickApi<EmptyRequest, EmptyResponse>
    {
    }
}