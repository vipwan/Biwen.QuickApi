using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi
{
    /// <summary>
    /// BaseQuickApi
    /// </summary>
    /// <typeparam name="Req">请求对象</typeparam>
    /// <typeparam name="Rsp">输出对象</typeparam>
    public abstract class BaseQuickApi<Req, Rsp> : IQuickApi<Req, Rsp>, IHandlerBuilder where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
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
            else if (RspType == typeof(IResultResponse))
            {
                //todo:IResultResponse不提供具体的类型，需执行时自行指定
            }
            else
            {
                if (RspType != typeof(EmptyResponse))
                    builder?.Produces(200, RspType);
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

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.Produces(200, null);
            return base.HandlerBuilder(builder);
        }

        public override Task<EmptyResponse> ExecuteAsync(Req request)
        {
            return Task.FromResult(EmptyResponse.New);
        }
    }

    /// <summary>
    /// 没有请求参数的BaseQuickApi,没有返回值
    /// </summary>
    public abstract class BaseQuickApi : BaseQuickApi<EmptyRequest, EmptyResponse>
    {
        public BaseQuickApi()
        {
            UseReqBinder<EmptyReqBinder<EmptyRequest>>();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.Produces(200, null);
            return base.HandlerBuilder(builder);
        }

        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(EmptyResponse.New);
        }
    }

    /// <summary>
    /// 没有请求参数的BaseQuickApi,有返回值
    /// </summary>
    /// <typeparam name="Rsp"></typeparam>
    public abstract class BaseQuickApiWithoutRequest<Rsp> : BaseQuickApi<EmptyRequest, Rsp> where Rsp : BaseResponse
    {
        public BaseQuickApiWithoutRequest()
        {
            UseReqBinder<EmptyReqBinder<EmptyRequest>>();
        }
    }

}
