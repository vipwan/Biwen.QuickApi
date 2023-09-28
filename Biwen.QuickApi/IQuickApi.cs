using Microsoft.AspNetCore.Builder;
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
        OPTIONS = 64,
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
        /// 提供minimal扩展,可以扩充缓存,日志,鉴权等功能,..
        /// 注意OpenApi的Produces<>QuickApi已经默认实现.
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
        internal Type ReqType => typeof(Req);
        /// <summary>
        ///  输出类型
        /// </summary>
        internal Type RspType => typeof(Rsp);

        /// <summary>
        /// 默认的请求参数绑定器
        /// </summary>
        private IReqBinder<Req> _reqBinder = new DefaultReqBinder<Req>();

        public IReqBinder<Req> ReqBinder
        {
            get => _reqBinder;
            private set => _reqBinder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 使用自定义的请求参数绑定器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UseReqBinder<T>() where T : IReqBinder<Req>, new()
        {
            ReqBinder = new T();
        }

        /// <summary>
        /// 请求输出,注意如果需要Request对象，请使用HttpContextAccessor.HttpContext.Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<Rsp> ExecuteAsync(Req request);

        public virtual RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //Accepts
            if (ReqType != typeof(EmptyRequest))
            {
                builder?.Accepts(ReqType, "application/json");
            }
            //200
            if (RspType == typeof(ContentResponse))
            {
                builder?.Produces(200, typeof(string), contentType: "text/plain");
            }
            else
            {
                builder?.Produces(200, RspType == typeof(EmptyResponse) ? null : RspType);
            }
            //400
            if (RspType != typeof(EmptyRequest))
            {
                builder?.ProducesValidationProblem();
            }
            //500
            builder?.ProducesProblem(StatusCodes.Status500InternalServerError);

            //todo:
            return builder!;
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