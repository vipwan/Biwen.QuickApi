// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 BaseQuickApi.cs

using Biwen.QuickApi.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace Biwen.QuickApi;

/// <summary>
/// BaseQuickApi
/// </summary>
/// <typeparam name="Req">请求对象</typeparam>
/// <typeparam name="Rsp">输出对象</typeparam>
public abstract class BaseQuickApi<Req, Rsp> : IQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new()
{
    /// <summary>
    /// 获取请求类型
    /// </summary>
    internal Type ReqType => typeof(Req);
    /// <summary>
    ///  输出类型
    /// </summary>
    internal Type RspType => typeof(Rsp);

    /// <inheritdoc cref="IsAntiforgeryEnabled" />
    public virtual bool IsAntiforgeryEnabled => false;

    public virtual async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        await @event.PublishAsync(cancellationToken);
    }

    ///// <summary>
    ///// 默认的请求参数绑定器
    ///// </summary>
    //private IReqBinder<Req> _reqBinder = new DefaultReqBinder<Req>();

    //public IReqBinder<Req> ReqBinder
    //{
    //    get => _reqBinder;
    //    private set => _reqBinder = value ?? throw new ArgumentNullException(nameof(value));
    //}

    private Type _reqBinder = typeof(DefaultReqBinder<Req>);

    public Type ReqBinder
    {
        get { return _reqBinder; }
        private set => _reqBinder = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// 使用自定义的请求参数绑定器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void UseReqBinder<T>() where T : class, IReqBinder<Req>
    {
        ReqBinder = typeof(T);
    }

    /// <summary>
    /// 执行请求,如需要HttpContext对象，请使用<see cref="IHttpContextAccessor"/>获取HttpContext
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract ValueTask<Rsp> ExecuteAsync(Req request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 处理IResult的特性
    /// </summary>
    static class ProducesIResult
    {
        static readonly MethodInfo _populateMethod =
            typeof(ProducesIResult).GetMethod(nameof(Populate), BindingFlags.NonPublic | BindingFlags.Static)!;

        public static void AddMetadata(EndpointBuilder builder, Type tResponse)
        {
            if (tResponse is not null && typeof(IEndpointMetadataProvider).IsAssignableFrom(tResponse))
            {
                _populateMethod.MakeGenericMethod(tResponse).Invoke(null, [builder]);
            }
        }

        static void Populate<T>(EndpointBuilder b) where T : IEndpointMetadataProvider
        {
            T.PopulateMetadata(_populateMethod, b);
        }
    }

    /*
     *  
     * AddEndpointFilter 提供诸如筛选器, https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis/min-api-filters
     * WithOpenApi  https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis/openapi
     * 日志,缓存,鉴权等功能,..
     * 
     */

    /// <summary>
    /// HandlerBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
    {
        // 200
        if (RspType == typeof(string))
        {
            builder?.Produces(200, typeof(string), contentType: "text/plain");
        }
        else if (RspType.IsAssignableTo(typeof(IResult)))
        {
            builder?.Add(eb => ProducesIResult.AddMetadata(eb, RspType));
        }
        else
        {
            builder?.Produces(200, RspType);
        }

        // multipart/form-data
        if (IsFormdata)
        {
            builder?.Accepts<Req>("multipart/form-data");
        }

        // todo:
        return builder!;
    }


    /// <summary>
    /// ReqType是否是:multipart/form-data
    /// </summary>
    static readonly ConcurrentDictionary<Type, bool> _reqTypeIsFormdatas = new();

    /// <summary>
    /// 缓存请求类型是否form-data,避免重复反射
    /// </summary>
    private bool IsFormdata
    {
        get
        {
            return _reqTypeIsFormdatas.GetOrAdd(ReqType, type =>
            {
                return (ReqType.GetProperties().Any(x =>
                x.PropertyType == typeof(IFormFile) ||
                x.PropertyType == typeof(IFormFileCollection)));
            });
        }
    }

    /// <summary>
    /// 请求QuickApi前的操作,推荐使用:<see cref="IEndpointFilter"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task BeforeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
    /// <summary>
    /// 请求QuickApi后的操作,推荐使用:<see cref="IEndpointFilter"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task AfterAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 撤销请求,针对长时间的ExecuteAsync(),如果需要中断,可以调用此方法
    /// </summary>
    /// <returns></returns>
    public async Task CancelAsync()
    {
        var asyncContext = ActivatorUtilities.GetServiceOrCreateInstance<AsyncContextService<CancellationTokenSource>>(
            ServiceRegistration.ServiceProvider);
        if (asyncContext?.TryGet(out var cts) is true && cts is not null)
        {
            await cts.CancelAsync();
        }
    }
}

/// <summary>
/// 有请求参数的BaseQuickApi,返回IResultResponse
/// </summary>
/// <typeparam name="Req"></typeparam>
public abstract class BaseQuickApi<Req> : BaseQuickApi<Req, IResult> where Req : BaseRequest<Req>, new()
{
    public abstract override ValueTask<IResult> ExecuteAsync(Req request, CancellationToken cancellationToken = default);
}

/// <summary>
/// 没有请求参数的BaseQuickApi,返回:<see cref="IResult"/>
/// </summary>
public abstract class BaseQuickApi : BaseQuickApi<EmptyRequest, IResult>
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

    public abstract override ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default);

}

/// <summary>
/// 没有请求参数的BaseQuickApi,有返回值
/// </summary>
/// <typeparam name="Rsp"></typeparam>
public abstract class BaseQuickApiWithoutRequest<Rsp> : BaseQuickApi<EmptyRequest, Rsp>
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
public abstract class BaseQuickApiJustAsService<Req, Rsp> : BaseQuickApi<Req, Rsp> where Req : BaseRequest<Req>, new()
{
}