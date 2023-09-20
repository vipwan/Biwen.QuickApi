using Microsoft.AspNetCore.Http;

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
    internal interface IQuickApi { }

    /// <summary>
    /// BaseQuickApi
    /// </summary>
    /// <typeparam name="Req">请求对象</typeparam>
    /// <typeparam name="Rsp">输出对象</typeparam>
    public abstract class BaseQuickApi<Req, Rsp> : IQuickApi where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
    {
        /// <summary>
        /// HttpContextAccessor
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;
        /// <summary>
        /// ServiceProvider
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; } = null!;
        /// <summary>
        /// 请求输出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Rsp Execute(Req request);

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