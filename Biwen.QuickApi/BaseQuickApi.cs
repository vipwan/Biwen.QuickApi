using Biwen.QuickApi.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSwag.Annotations;

namespace Biwen.QuickApi
{
    using Microsoft.Extensions.DependencyInjection.Extensions;
    /// <summary>
    /// BaseQuickApi
    /// </summary>
    /// <typeparam name="Req">请求对象</typeparam>
    /// <typeparam name="Rsp">输出对象</typeparam>
    public abstract class BaseQuickApi<Req, Rsp> : IQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
    {
        /// <summary>
        /// 获取请求类型
        /// </summary>
        internal Type ReqType => typeof(Req);
        /// <summary>
        ///  输出类型
        /// </summary>
        internal Type RspType => typeof(Rsp);

        /// <inheritdoc cref="IAntiforgery.IsAntiforgeryEnabled" />
        public virtual bool IsAntiforgeryEnabled => false;

        public virtual async Task PublishAsync<T>(T @event) where T : IEvent
        {
            using var scope = ServiceRegistration.ServiceProvider.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<Publisher>();
            await publisher.PublishAsync(@event);
        }

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
        /// 执行请求,如需要HttpContext对象，请使用<see cref="IHttpContextAccessor.HttpContext"/>获取
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract ValueTask<Rsp> ExecuteAsync(Req request);



        /*
         *  
         * AddEndpointFilter 提供诸如筛选器, https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis/min-api-filters
         * WithOpenApi  https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis/openapi
         * 日志,缓存,鉴权等功能,..
         * 
         */

        /// <summary>
        /// https://github.com/RicoSuter/NSwag/issues/4163
        /// 请注意NSwag和AspnetCore-OpenApi的WithOpenApi(Summary,Description)不兼容,
        /// 请使用<see cref="OpenApiOperationAttribute"/>标记<seealso cref="BaseQuickApi.HandlerBuilder"/>方法!
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public virtual RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //Accepts
            //if (ReqType != typeof(EmptyRequest))
            //{
            //    builder?.Accepts(ReqType, "application/json");
            //}
            // 200
            if (RspType == typeof(ContentResponse))
            {
                builder?.Produces(200, typeof(string), contentType: "text/plain");
            }
            else if (RspType == typeof(IResultResponse))
            {
                // todo:IResultResponse不提供具体的类型，需执行时自行指定
            }
            else
            {
                if (RspType != typeof(EmptyResponse))
                    builder?.Produces(200, RspType);
            }
            // 400
            if (RspType != typeof(EmptyRequest))
            {
                builder?.ProducesValidationProblem();
            }
            // 500
            builder?.ProducesProblem(StatusCodes.Status500InternalServerError);

            // 上传文件必须使用 multipart/form-data
            if (ReqType.GetProperties().Any(x =>
                x.PropertyType == typeof(IFormFile) ||
                x.PropertyType == typeof(IFormFileCollection)))
            {
                builder?.Accepts<Req>("multipart/form-data");
            }

            // todo:
            return builder!;
        }

        private static readonly string? AssemblyName = typeof(BaseQuickApi<Req, Rsp>).Assembly.GetName().Name;

        /// <summary>
        /// 请求QuickApi前的操作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public virtual Task InvokeBeforeAsync(HttpContext context)
        {
            context.Response.Headers.TryAdd("X-Powered-By", AssemblyName);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 请求QuickApi后的操作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public virtual Task InvokeAfterAsync(HttpContext context)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 有请求参数的BaseQuickApi,返回IResultResponse
    /// </summary>
    /// <typeparam name="Req"></typeparam>
    public abstract class BaseQuickApi<Req> : BaseQuickApi<Req, IResultResponse> where Req : BaseRequest<Req>, new()
    {
        public abstract override ValueTask<IResultResponse> ExecuteAsync(Req request);
    }

    /// <summary>
    /// 没有请求参数的BaseQuickApi,没有返回值
    /// </summary>
    public abstract class BaseQuickApi : BaseQuickApi<EmptyRequest, IResultResponse>
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

        public abstract override ValueTask<IResultResponse> ExecuteAsync(EmptyRequest request);

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


    /// <summary>
    /// BaseQuickApi 作为服务使用,不注册Api路由
    /// </summary>
    /// <typeparam name="Req"></typeparam>
    /// <typeparam name="Rsp"></typeparam>
    [QuickApi(""), JustAsService]
    public abstract class BaseQuickApiJustAsService<Req, Rsp> : BaseQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new() where Rsp : BaseResponse
    {

    }




}