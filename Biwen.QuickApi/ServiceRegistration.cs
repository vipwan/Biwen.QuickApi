using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using System.Dynamic;

namespace Biwen.QuickApi
{
    using Biwen.QuickApi.Abstractions;
    using Biwen.QuickApi.Events;
    using Biwen.QuickApi.Http;
#if NET8_0_OR_GREATER
    using Microsoft.AspNetCore.Antiforgery;
#endif
    using Microsoft.AspNetCore.Http.Json;

    public static class ServiceRegistration
    {
        /// <summary>
        /// sp
        /// </summary>
        internal static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// Add Biwen.QuickApis,默认Json序列化JsonSerializerDefaults.Web.
        /// 你也可以自行调用配置更多选项<see cref="HttpJsonServiceExtensions.ConfigureHttpJsonOptions"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBiwenQuickApis(
            this IServiceCollection services,
            Action<BiwenQuickApiOptions>? options = null)
        {

            //JSON Options
            services.ConfigureHttpJsonOptions(x => { });

            //注册验证器
            services.AddFluentValidationAutoValidation();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            //注册Antiforgery服务
            services.AddAntiforgery();

            //重写AuthorizationMiddlewareResultHandler
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, QuickApiAuthorizationMiddlewareResultHandler>();
            //默认的异常返回构造器
            services.AddSingleton<IQuickApiExceptionResultBuilder, DefaultExceptionResultBuilder>();
            //options
            services.AddOptions<BiwenQuickApiOptions>().Configure(o => { options?.Invoke(o); });

            /// <summary>
            /// 开启ProblemDetails
            /// 如需自定义请调用
            /// <see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection, Action{ProblemDetailsOptions}?)"/>
            /// <seealso cref="ProblemDetailsOptions"/>
            /// 500错误推荐重写并注册:<seealso cref="IQuickApiExceptionResultBuilder"/>
            /// </summary>
            //AddProblemDetails
            services.AddProblemDetails();

            //注册EventHanders
            foreach (var subscriberType in EventSubscribers)
            {
                var baseType = subscriberType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == InterfaceEventSubscriber);
                services.AddScoped(baseType, subscriberType);
            }
            //注册Publisher
            services.AddScoped<Publisher>();

            //add quickapis
            foreach (var api in Apis) services.AddScoped(api);


            return services;
        }

        /// <summary>
        /// 添加对Group的的扩展支持
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBiwenQuickApiGroupRouteBuilder<T>(this IServiceCollection services) where T : IQuickApiGroupRouteBuilder, new()
        {
            services.AddSingleton<IQuickApiGroupRouteBuilder>((sp) => { return new T(); });
            return services;
        }

        #region internal

        /// <summary>
        /// 拆分枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        static List<T> SplitEnum<T>(this T e) where T : Enum
        {
            var result = new List<T>();
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if ((Convert.ToInt32(item) & Convert.ToInt32(e)) > 0)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        static readonly Type InterfaceQuickApi = typeof(IQuickApi<,>);
        static readonly Type InterfaceReqBinder = typeof(IReqBinder<>);
        static readonly Type InterfaceEventSubscriber = typeof(IEventSubscriber<>);

        static readonly object _lock = new();//锁

        static bool IsToGenericInterface(this Type type, Type baseInterface)
        {
            if (type == null) return false;
            if (baseInterface == null) return false;

            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == baseInterface);
        }

        static IEnumerable<Type> _apis = null!;
        /// <summary>
        /// 所有的QuickApi
        /// </summary>
        static IEnumerable<Type> Apis
        {
            get
            {
                lock (_lock)
                    return _apis ??= ASS.InAllRequiredAssemblies.Where(x =>
                    !x.IsAbstract && x.IsPublic && x.IsClass && x.IsToGenericInterface(InterfaceQuickApi));
            }
        }

        static IEnumerable<Type> _binders = null!;
        static IEnumerable<Type> Binders
        {
            get
            {
                lock (_lock)
                    return _binders ??= ASS.InAllRequiredAssemblies.Where(x =>
                    !x.IsAbstract && x.IsPublic && x.IsClass && x.IsToGenericInterface(InterfaceReqBinder));
            }
        }


        static IEnumerable<Type> _eventSubscribers = null!;

        static IEnumerable<Type> EventSubscribers
        {
            get
            {
                lock (_lock)
                    return _eventSubscribers ??= ASS.InAllRequiredAssemblies.Where(x =>
                    !x.IsAbstract && x.IsPublic && x.IsClass && x.IsToGenericInterface(InterfaceEventSubscriber));
            }
        }


        #endregion

        /// <summary>
        /// Map Biwen.QuickApis
        /// 推荐安装Biwen.QuickApi.SourceGenerator代码生成器调用:app.MapGenQuickApis();
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="QuickApiExcetion"></exception>
        public static (string Group, RouteGroupBuilder RouteGroupBuilder)[] MapBiwenQuickApis(this IEndpointRouteBuilder app)
        {
            if (!Apis.Any())
            {
                throw new QuickApiExcetion("确定你有添加任何继承了BaseQuickApi的Api吗!?");
            }

            if (Apis.Any(x => x.GetCustomAttribute<QuickApiAttribute>() == null))
            {
                throw new QuickApiExcetion($"所有QuickApi都必须标注QuickApi特性!");
            }

            //sp
            ServiceRegistration.ServiceProvider = app.ServiceProvider;

            var quickApiOptions = app.ServiceProvider.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value;

            //antiforgery middleware
            if (quickApiOptions.EnableAntiForgeryTokens)
            {
#if !NET8_0_OR_GREATER
                (app as WebApplication)?.UseMiddleware<QuickApiAntiforgeryMiddleware>();
#endif
#if NET8_0_OR_GREATER
                (app as WebApplication)?.UseAntiforgery();
#endif
            }
            //middleware:
            (app as WebApplication)?.UseMiddleware<QuickApiMiddleware>();

            //分组:
            var groups = Apis.GroupBy(x => x.GetCustomAttribute<QuickApiAttribute>()!.Group.ToLower());
            var routeGroups = new List<(string, RouteGroupBuilder)>();

            //quickapi前缀
            var prefix = quickApiOptions.RoutePrefix;
            foreach (var group in groups)
            {
                var g = app.MapGroup(string.Empty);
                //quickapi前缀
                if (!string.IsNullOrEmpty(prefix))
                {
                    g = g.MapGroup(prefix);
                }
                if (!string.IsNullOrEmpty(group.Key))
                {
                    //url分组
                    g = g.MapGroup(group.Key);
                }

                //GroupRouteBuilder
                var groupRouteBuilders = app.ServiceProvider.GetServices<IQuickApiGroupRouteBuilder>();
                foreach (var groupRouteBuilder in groupRouteBuilders.OrderBy(x => x.Order))
                {
                    if (groupRouteBuilder.Group.Equals(group.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        g = groupRouteBuilder.Builder(g);
                    }
                }

                foreach (var apiType in group)
                {
                    var attr = apiType.GetCustomAttribute<QuickApiAttribute>() ?? throw new QuickApiExcetion($"{apiType.Name}:必须标注QuickApi特性!");

                    if (apiType.GetCustomAttribute<JustAsServiceAttribute>() != null)
                    {
                        //不需要注册路由的QuickApi
                        continue;
                    }
                    var verbs = attr.Verbs.SplitEnum();//拆分枚举

                    //MapMethods
                    var rhBuilder = g.MapMethods(attr.Route, verbs.Select(x => x.ToString()).ToArray(),
                       async Task<IResult> (IHttpContextAccessor ctx) =>
                        {
                            return await RequestHandler(ctx, apiType, attr);
                        });

                    //HandlerBuilder
                    using var scope = app.ServiceProvider.CreateAsyncScope();
                    var hb = scope.ServiceProvider.GetRequiredService(apiType) as IHandlerBuilder;
                    rhBuilder = hb!.HandlerBuilder(rhBuilder);

                    //metadata
                    rhBuilder.WithMetadata(new QuickApiMetadata(apiType));
                    //antiforgery
                    //net8.0以上使用UseAntiforgery,
                    //net7.0以下使用QuickApiAntiforgeryMiddleware
                    var antiforgeryApi = scope.ServiceProvider.GetRequiredService(apiType) as IAntiforgeryApi;
#if NET8_0_OR_GREATER
                    if (antiforgeryApi?.IsAntiforgeryEnabled is false)
                    {
                        rhBuilder.DisableAntiforgery();
                    }
                    if (antiforgeryApi?.IsAntiforgeryEnabled is true)
                    {
                        if (!quickApiOptions.EnableAntiForgeryTokens)
                        {
                            throw new QuickApiExcetion($"如需要防伪验证,请启用BiwenQuickApiOptions.EnableAntiForgeryTokens!");
                        }
                        rhBuilder.WithMetadata(new RequireAntiforgeryTokenAttribute(true));
                    }
#endif
                    //outputcache
                    var outputCacheAttribute = apiType.GetCustomAttribute<OutputCacheAttribute>();
                    if (outputCacheAttribute != null)
                    {
                        rhBuilder.WithMetadata(outputCacheAttribute);
                    }

                    //endpointgroup
                    var endpointgroupAttribute = apiType.GetCustomAttribute<EndpointGroupNameAttribute>();
                    if (endpointgroupAttribute != null)
                    {
                        rhBuilder.WithMetadata(endpointgroupAttribute);
                    }
                    //authorizeattribute
                    var authorizeAttributes = apiType.GetCustomAttributes<AuthorizeAttribute>();
                    if (authorizeAttributes.Any()) rhBuilder.WithMetadata(new AuthorizeAttribute());
                    foreach (var authAttr in authorizeAttributes)
                    {
                        rhBuilder.WithMetadata(authAttr);
                    }
                    //allowanonymous
                    var allowanonymous = apiType.GetCustomAttribute<AllowAnonymousAttribute>();
                    if (allowanonymous != null) rhBuilder.WithMetadata(allowanonymous);

                    //return rhBuilder.RequireAuthorization(policyNames.Select(n => new AuthorizeAttribute(n)).ToArray());

                    //OpenApi 生成
                    //var method = apiType.GetMethod("ExecuteAsync")!;
                    //var parameter = method.GetParameters()[0]!;
                    //var parameterType = parameter.ParameterType!;

                    //var parameterType = ((dynamic)currentApi).ReqType as Type;
                    //if (!verbs.Contains(Verb.GET) && parameterType != typeof(EmptyRequest))
                    //{
                    //    rhBuilder!.Accepts(parameterType!, "application/json");
                    //}

                    //401
                    if (!string.IsNullOrEmpty(attr.Policy))
                    {
                        rhBuilder?.ProducesProblem(StatusCodes.Status401Unauthorized);
                    }
                }
                routeGroups.Add((group.Key, g));
            }

            return routeGroups.ToArray();
        }

        /// <summary>
        /// IApplicationBuilder.UseBiwenQuickApis();
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBiwenQuickApis(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBiwenQuickApis();
            });
            return app;
        }

        /// <summary>
        /// 执行请求的委托
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="apiType"></param>
        /// <param name="quickApiAttribute"></param>
        /// <returns></returns>
        /// <exception cref="QuickApiExcetion"></exception>
        async static Task<IResult> RequestHandler(IHttpContextAccessor ctx, Type apiType, QuickApiAttribute quickApiAttribute)
        {
            if (ctx == null) throw new QuickApiExcetion($"HttpContextAccessor is null!");
            if (apiType == null) throw new QuickApiExcetion($"apiType is null!");
            if (quickApiAttribute == null) throw new QuickApiExcetion($"quickApiAttribute is null!");

            //验证策略
            var checkResult = await CheckPolicy(ctx, quickApiAttribute.Policy);
            if (!checkResult.Flag) return checkResult.Result!;

            var sp = ctx.HttpContext!.RequestServices;

            var api = sp.GetRequiredService(apiType);
            var quickApiOptions = sp.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value;
            //使用系统自带的JsonSerializerOptions配置
            var jsonSerializerOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;

            //执行请求
            try
            {
                //var cache = ctx.HttpContext!.RequestServices.GetRequiredService<IMemoryCache>();
                //var method = apiType.GetMethod("ExecuteAsync")!;
                //var parameter = method.GetParameters()[0]!;
                //var parameterType = parameter.ParameterType!;
                var req = await ((dynamic)api).ReqBinder.BindAsync(ctx.HttpContext!);

                //验证DTO
                if (req.Validate() is ValidationResult vresult && !vresult!.IsValid)
                {
                    return TypedResults.ValidationProblem(vresult.ToDictionary());
                }

                //var result = await method.Invoke(api, new object[] { req! })!;
                var result = await ((dynamic)api)!.ExecuteAsync(req!);
                var resultFlag = InnerResult(result as BaseResponse);
                if (resultFlag.Flag) return resultFlag.Result!;

                //针对返回结果的别名处理
                Func<dynamic?, dynamic?> rspToExpandoObject = (rsp) =>
                {
                    if (rsp == null) return null;

                    var type = rsp.GetType() as Type;

                    var hasAlias = type!.GetProperties().Any(x => x.GetCustomAttribute<AliasAsAttribute>() != null);
                    if (!hasAlias)
                    {
                        return rsp;
                    }

                    dynamic expandoObject = new ExpandoObject();
                    foreach (var prop in type.GetProperties())
                    {
                        if (prop is null) continue;
                        var alias = prop.GetCustomAttribute<AliasAsAttribute>();
                        ((IDictionary<string, object>)expandoObject)[alias != null ? alias.Name : prop.Name] = prop.GetValue(rsp)!;
                    }
                    return expandoObject;
                };

                //返回JSON
                var expandoResult = rspToExpandoObject(result);
                //使用JsonSerializerOptions
                return TypedResults.Json(expandoResult, jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                var exceptionHandlers = sp.GetServices<IQuickApiExceptionHandler>();
                //异常处理
                foreach (var handler in exceptionHandlers)
                {
                    await handler.HandleAsync(ex);
                }
                //规范化异常返回
                var exceptionResultBuilder = sp.GetRequiredService<IQuickApiExceptionResultBuilder>();
                return await exceptionResultBuilder.ErrorResult(ex);
            }
        }

        /// <summary>
        /// 验证Policy
        /// </summary>
        /// <exception cref="QuickApiExcetion"></exception>
        async static Task<(bool Flag, IResult? Result)> CheckPolicy(IHttpContextAccessor ctx, string? policy)
        {
            if (string.IsNullOrEmpty(policy))
            {
                return (true, null);
            }
            if (!string.IsNullOrEmpty(policy))
            {
                var httpContext = ctx.HttpContext;
                var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null, besure services.AddAuthorization() first!");
                var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                if (!authorizationResult.Succeeded)
                {
                    return (false, TypedResults.Unauthorized());
                }
            }
            return (true, null);
        }

        /// <summary>
        /// 内部返回的Result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        static (bool Flag, IResult? Result) InnerResult(BaseResponse? result)
        {
            //返回空结果
            if (result is EmptyResponse)
            {
                return (true, TypedResults.Ok());//返回空
            }
            //返回文本结果
            if (result is ContentResponse content)
            {
                return (true, TypedResults.Content(content.ToString()));
            }
            //返回IResult结果
            if (result is IResultResponse iresult)
            {
                return (true, iresult.Result);
            }
            return (false, null);
        }
    }
}