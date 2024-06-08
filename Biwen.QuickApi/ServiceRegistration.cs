using Biwen.QuickApi.Http;
using Biwen.QuickApi.OpenApi;

using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Biwen.QuickApi
{
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
            //解决utf-8编码问题:
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            //add RazorComponents
            services.AddRazorComponents().AddInteractiveServerComponents();

            //JSON Options
            services.ConfigureHttpJsonOptions(x => { });

            //注册验证器
            services.AddFluentValidationAutoValidation();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            //注册Antiforgery服务
            services.AddAntiforgery();

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

            //注册模块
            services.AddModular();

            return services;
        }

        //注册模块
        private static IServiceCollection AddModular(this IServiceCollection services)
        {
            using var sp = services.BuildServiceProvider().CreateScope();
            foreach (var modularType in Modulars)
            {
                if (ActivatorUtilities.CreateInstance(sp.ServiceProvider, modularType) is ModularBase { } modular && modular.IsEnable())
                {
                    modular.ConfigureServices(services);
                    //DI
                    services.AddTransient(typeof(IStartup), modularType);
                }
            }
            return services;
        }

        /// <summary>
        /// 添加对Group的的扩展支持
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuickApiGroupRouteBuilder<T>(this IServiceCollection services) where T : class, IQuickApiGroupRouteBuilder
        {
            services.AddSingleton<IQuickApiGroupRouteBuilder, T>();
            return services;
        }

        #region internal

        //static readonly Type InterfaceQuickApi = typeof(IQuickApi<,>);
        //static readonly Type InterfaceReqBinder = typeof(IReqBinder<>);
        //static readonly Type InterfaceEventSubscriber = typeof(IEventSubscriber<>);
        static readonly Type ModularBaseType = typeof(ModularBase);
        static readonly object _lock = new();//锁

        static IEnumerable<Type> _modulars = null!;
        static IEnumerable<Type> Modulars
        {
            get
            {
                lock (_lock)
                {
                    return _modulars ??= ASS.InAllRequiredAssemblies.Where(x => !x.IsAbstract && x.IsClass && x.IsAssignableTo(ModularBaseType));
                }
            }
        }

        static List<Type> configedTypes = new();
        static void Configure(IStartup modularType, IApplicationBuilder app, IEndpointRouteBuilder routes)
        {
            lock (_lock)
            {
                if (configedTypes.Contains(modularType.GetType()))
                    return;

                var pres = modularType.GetType().GetCustomAttributes(typeof(PreModularAttribute<>));
                if (pres.Any() is true)
                {
                    foreach (var pre in pres)
                    {
                        //获取泛型参数
                        var preType = pre.GetType().GetGenericArguments().First();
                        var preModular = app.ApplicationServices.GetServices<IStartup>().First(x => x.GetType() == preType);
                        Configure(preModular, app, routes);
                    }
                }

                modularType.Configure(app, routes, app.ApplicationServices);
                configedTypes.Add(modularType.GetType());
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
            if (!_CoreModular.Apis.Any())
            {
                return default!;
            }

            if (_CoreModular.Apis.Any(x => x.GetCustomAttribute<QuickApiAttribute>() == null))
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

            //openapi doc middleware
#pragma warning disable CS0618 // 类型或成员已过时
            (app as WebApplication)?.UseMiddleware<OpenApiDocMiddleware>();
#pragma warning restore CS0618 // 类型或成员已过时

            //middleware:
            (app as WebApplication)
                //QuickApiMiddleware
                ?.UseMiddleware<QuickApiMiddleware>()
                //ExceptionHandler
                .UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                => await Results.Problem().ExecuteAsync(context)));

            //分组:
            var groups = _CoreModular.Apis.GroupBy(x => x.GetCustomAttribute<QuickApiAttribute>()!.Group.ToLower());
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
                    //reqType
                    var reqType = apiType.GetMethod(nameof(BaseQuickApi.ExecuteAsync))!.GetParameters()[0].ParameterType;

                    //MapMethods
                    var rhBuilder = g.MapMethods(attr.Route, verbs.Select(x => x.ToString()),
                       async (IHttpContextAccessor ctx) =>
                        {
                            return await ProcessRequestAsync(ctx, apiType, attr);
                        });

                    //Accept
                    rhBuilder?.WithMetadata(new AcceptsMetadata(["application/json"], reqType));

                    //HandlerBuilder
                    using var scope = app.ServiceProvider.CreateAsyncScope();
                    var hb = scope.ServiceProvider.GetRequiredService(apiType) as IHandlerBuilder;
                    rhBuilder = hb!.HandlerBuilder(rhBuilder!);

                    //metadata
                    rhBuilder.WithMetadata(new QuickApiMetadata(apiType, attr));

                    //验证策略
                    rhBuilder.AddEndpointFilter<CheckPolicyFilter>();

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

                    //获取T的所有Attribute:
                    var attrs = apiType.GetCustomAttributes(true);
                    //将所有的Attribute添加到metadatas中
                    rhBuilder?.WithMetadata(attrs);

                    //OpenApiMetadataAttribute
                    var openApiMetadata = apiType.GetCustomAttribute<OpenApiMetadataAttribute>();
                    if (openApiMetadata is { })
                    {
                        if (openApiMetadata.Tags.Length > 0)
                        {
                            rhBuilder?.WithTags(openApiMetadata.Tags);
                        }
                        if (!string.IsNullOrEmpty(openApiMetadata.Summary))
                        {
                            rhBuilder?.WithSummary(openApiMetadata.Summary);
                        }
                        if (!string.IsNullOrEmpty(openApiMetadata.Description))
                        {
                            rhBuilder?.WithDescription(openApiMetadata.Description);
                        }
                        //兼容性问题,Verbs数量>1将不会添加OperationId等信息
                        if (verbs.Count() == 1)
                        {
                            rhBuilder?.WithOpenApi(operation => new(operation)
                            {
                                Deprecated = openApiMetadata.IsDeprecated,
                                OperationId = openApiMetadata.OperationId,
                                //参数的备注和example等:
                                //Parameters = GetParameters(reqType),

                                //RequestBody = new OpenApiRequestBody
                                //{
                                //    Required = true,
                                //    Content =
                                //    {
                                //        {
                                //            "application/json",
                                //            new OpenApiMediaType
                                //            {
                                //               Schema=new OpenApiSchema
                                //               {
                                //                   Type="object",
                                //                   Properties=GetParameters(reqType).ToDictionary(x=>x.Name,x=>new OpenApiSchema
                                //                   {
                                //                       Type=x.Name switch
                                //                       {
                                //                           "Hello"=>"string",
                                //                           "World"=>"string",
                                //                           _=>"string"
                                //                       }
                                //                   })
                                //               }
                                //        }
                                //    }

                                //}
                                //}
                            });
                        }
                    }

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

        private const string EndpointRouteBuilder = "__EndpointRouteBuilder";

        /// <summary>
        /// IApplicationBuilder.UseBiwenQuickApis();
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBiwenQuickApis(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthorization();

            // Try to retrieve the current 'IEndpointRouteBuilder'.
            if (!app.Properties.TryGetValue(EndpointRouteBuilder, out var obj) ||
                obj is not IEndpointRouteBuilder routes)
            {
                throw new InvalidOperationException("Failed to retrieve the current endpoint route builder.");
            }

            routes.MapBiwenQuickApis();

            //内核模块:
            var coreModulars = app.ApplicationServices.GetServices<IStartup>()
                .Where(x => x.GetType().GetCustomAttribute<CoreModularAttribute>() is { })
                .OrderBy(s => s.Order);

            //非内核模块:
            var normalModulars = app.ApplicationServices.GetServices<IStartup>()
                .Where(x => x.GetType().GetCustomAttribute<CoreModularAttribute>() is null)
                .OrderBy(s => s.Order);

            foreach (var modularType in coreModulars.Concat(normalModulars))
            {
                Configure(modularType, app, routes);
            }

            // Knowing that routes are already configured.
            app.UseEndpoints(routes => { });
            app.UseStaticFiles();

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
        async static Task<IResult> ProcessRequestAsync(IHttpContextAccessor ctx, Type apiType, QuickApiAttribute quickApiAttribute)
        {
            if (ctx == null) throw new QuickApiExcetion($"HttpContextAccessor is null!");
            if (apiType == null) throw new QuickApiExcetion($"apiType is null!");
            if (quickApiAttribute == null) throw new QuickApiExcetion($"quickApiAttribute is null!");
            var sp = ctx.HttpContext!.RequestServices;
            var api = sp.GetRequiredService(apiType);
            //执行请求
            try
            {
                //使用接口静态成员重写代码:
                var methodName = nameof(IReqBinder<EmptyRequest>.BindAsync);
                MethodInfo methodInfo = (((dynamic)api).ReqBinder).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                var req = await (dynamic)methodInfo.Invoke(null, [ctx.HttpContext, null])!;
                if (req is not null)
                {
                    //验证DTO
                    if (req.Validate() is ValidationResult { IsValid: false } vresult)
                    {
                        return TypedResults.ValidationProblem(vresult.ToDictionary());
                    }
                }
                var result = await ((dynamic)api)!.ExecuteAsync(req!);
                var rawResult = InnerResult(result);
                return rawResult;
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
                var exceptionResultBuilder = sp.GetService<IQuickApiExceptionResultBuilder>();
                if (exceptionResultBuilder is not null)
                {
                    return await exceptionResultBuilder.ErrorResult(ex);
                }
                //默认使用ProblemDetails
                throw;
            }

            /// <summary>
            /// 内部返回的Result
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            static IResult InnerResult(object? result) => result switch
            {
                null => TypedResults.Ok(),
                IResult iresult => iresult,
#pragma warning disable CS0618
                IResultResponse iresult => iresult.Result,
#pragma warning restore CS0618
                string iresult => TypedResults.Content(iresult),
                _ => TypedResults.Json(result),
            };
        }
    }
}