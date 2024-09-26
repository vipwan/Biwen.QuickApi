// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:09:03 ServiceRegistration.cs

using Biwen.QuickApi.Http;
using Biwen.QuickApi.Serializer;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;

namespace Biwen.QuickApi;

[SuppressType]
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
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddBiwenQuickApis(
        this IServiceCollection services,
        Action<BiwenQuickApiOptions>? options = null)
    {
        //解决utf-8编码问题:
        services.Configure<WebEncoderOptions>(options =>
        {
            options.TextEncoderSettings = new TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);
        });

        //add authorization
        services.AddAuthorization();

        //add authentication
        services.AddAuthentication().AddCookie().AddBearerToken();

        //add RazorComponents
        services.AddRazorComponents().AddInteractiveServerComponents();

        //add HttpClient
        services.AddHttpClient();//default httpclient

        //JSON Options
        services.ConfigureHttpJsonOptions(x => { });

        //注册验证器
        services.AddValidatorsFromAssemblies(Assemblies.AllRequiredAssemblies);

        //作用域范围内缓存服务提供程序
        services.AddScoped<ICachedServiceProvider, CachedServiceProvider>();
        //注册AsyncStateHttpContext & AsyncContextService
        services.AddAsyncStateHttpContext().TryAddSingleton(typeof(AsyncContextService<>));

        //注册Antiforgery服务
        services.AddAntiforgery();

        //options
        services.Configure<BiwenQuickApiOptions>(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection(BiwenQuickApiOptions.Key))
        .Configure<BiwenQuickApiOptions>(o => options?.Invoke(o));

        /// <summary>
        /// 开启ProblemDetails
        /// 如需自定义请调用
        /// <see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection, Action{ProblemDetailsOptions}?)"/>
        /// <seealso cref="ProblemDetailsOptions"/>
        /// 500错误推荐重写并注册:<seealso cref="IQuickApiExceptionResultBuilder"/>
        /// </summary>
        //AddProblemDetails
        services.AddProblemDetails();

        //添加序列化器
        services.AddSystemTextJsonSerializer();

        //注册模块
        services.AddModular();

        return services;
    }

    //注册模块
    private static IServiceCollection AddModular(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        foreach (var modularType in Modulars)
        {
            if (ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, modularType) is ModularBase { } modular && modular.IsEnable())
            {
                modular.ConfigureServices(services);
                //DI
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IStartup), modularType));
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
        //services.AddSingleton<IQuickApiGroupRouteBuilder, T>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IQuickApiGroupRouteBuilder, T>());
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
                return _modulars ??= ASS.InAllRequiredAssemblies.ThatInherit<ModularBase>()
                    .Where(x => !x.IsAbstract && x.IsClass);
            }
        }
    }

    static HashSet<Type> configedTypes = new();
    static void Configure(IStartup modular, IApplicationBuilder app, IEndpointRouteBuilder routes, ILogger? logger)
    {
        lock (_lock)
        {
            if (!modular.IsEnable())
            {
                return;
            }

            var modularType = modular.GetType();

            if (configedTypes.Contains(modularType))
                return;

            var preConfigure = (Attribute? attribute) =>
            {
                if (attribute is null) return;
                var preTypes = attribute.GetType().GetGenericArguments();
                foreach (var preType in preTypes)
                {
                    if (app.ApplicationServices.GetServices<IStartup>()
                    .FirstOrDefault(x => x.GetType() == preType) is { } preModular)
                    {
                        Configure(preModular, app, routes, logger);
                        configedTypes.Add(preModular.GetType());
                    }
                }
            };

            List<Attribute?> allPres = [
                modularType.GetCustomAttribute(typeof(PreModularAttribute<>)),
                modularType.GetCustomAttribute(typeof(PreModularAttribute<,>)),
                modularType.GetCustomAttribute(typeof(PreModularAttribute<,,>)),
                modularType.GetCustomAttribute(typeof(PreModularAttribute<,,,>)),
                modularType.GetCustomAttribute(typeof(PreModularAttribute<,,,,>)),
                modularType.GetCustomAttribute(typeof(PreModularAttribute<,,,,,>)),
                modularType.GetCustomAttribute(typeof(PreModularAttribute<,,,,,,>))
                ];
            allPres.ForEach(preConfigure);

            modular.Configure(app, routes, app.ApplicationServices);
            logger?.LogDebug($"Modular：{modularType.FullName} is Configured!");
            configedTypes.Add(modularType);
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
        if (!HttpModular.Apis.Any())
        {
            throw new QuickApiExcetion($"未找到任何QuickApi!,请添加一个HelloWorldApi吧 (#^.^#)");
        }
        if (HttpModular.Apis.Any(x => x.GetCustomAttribute<QuickApiAttribute>() == null))
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
        (app as WebApplication)
            //QuickApiMiddleware
            ?.UseMiddleware<QuickApiMiddleware>()
            //ExceptionHandler
            .UseExceptionHandler(exceptionHandlerApp
            => exceptionHandlerApp.Run(async context
            => await Results.Problem().ExecuteAsync(context)));

        //分组:
        var groups = HttpModular.Apis.GroupBy(x => x.GetCustomAttribute<QuickApiAttribute>()!.Group.ToLower());
        var routeGroups = new List<(string, RouteGroupBuilder)>();

        //quickapi前缀
        var prefix = quickApiOptions.RoutePrefix;
        foreach (var group in groups)
        {
            var g = app.MapGroup(string.Empty);
            //quickapi前缀
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                g = g.MapGroup(prefix);
            }
            if (!string.IsNullOrWhiteSpace(group.Key))
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
                if (apiType.GetCustomAttribute<JustAsServiceAttribute>() is { })
                {
                    //不需要注册路由的QuickApi
                    continue;
                }

                var attr = apiType.GetCustomAttribute<QuickApiAttribute>() ?? throw new QuickApiExcetion($"{apiType.Name}:必须标注QuickApi特性!");
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
                //审计
                rhBuilder.AddEndpointFilter<AuditApiFilter>();

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
                if (apiType.GetCustomAttribute<OpenApiMetadataAttribute>() is { } openApiMetadata)
                {
                    if (openApiMetadata.Tags.Length > 0)
                        rhBuilder?.WithTags(openApiMetadata.Tags);
                    if (!string.IsNullOrEmpty(openApiMetadata.Summary))
                        rhBuilder?.WithSummary(openApiMetadata.Summary);
                    if (!string.IsNullOrEmpty(openApiMetadata.Description))
                        rhBuilder?.WithDescription(openApiMetadata.Description);

#if NET9_0_OR_GREATER

                    //兼容性问题,Verbs数量>1将不会添加OperationId等信息
                    if (verbs.Count() == 1)
                    {
                        rhBuilder?.WithOpenApi(operation => new(operation)
                        {
                            Deprecated = openApiMetadata.IsDeprecated || apiType.GetCustomAttribute<ObsoleteAttribute>() is { },
                            OperationId = openApiMetadata.OperationId,
                            //参数的备注和example等:
                            //Parameters = GetParameters(reqType),
                        });
                    }
#endif

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

        // 认证鉴权中间件必须在UseRouting和UseEndpoints之间
        app.UseAuthentication();
        app.UseAuthorization();

        // Try to retrieve the current 'IEndpointRouteBuilder'.
        if (!app.Properties.TryGetValue(EndpointRouteBuilder, out var obj) ||
            obj is not IEndpointRouteBuilder routes)
        {
            throw new InvalidOperationException("Failed to retrieve the current endpoint route builder.");
        }

        //内核模块:
        var coreModulars = app.ApplicationServices.GetServices<IStartup>()
            .Where(x => x.GetType().GetCustomAttribute<CoreModularAttribute>() is { })
            .OrderBy(s => s.Order);

        //非内核模块:
        var normalModulars = app.ApplicationServices.GetServices<IStartup>()
            .Where(x => x.GetType().GetCustomAttribute<CoreModularAttribute>() is null)
            .OrderBy(s => s.Order);

        var logger = app.ApplicationServices.GetService<ILogger<ModularBase>>();
        foreach (var modular in coreModulars.Concat(normalModulars))
        {
            Configure(modular, app, routes, logger);
        }

        routes.MapBiwenQuickApis();

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
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(apiType);
        ArgumentNullException.ThrowIfNull(quickApiAttribute);

        var sp = ctx.HttpContext!.RequestServices;
        var bindService = sp.GetRequiredService<RequestBindService>();

        var tokenSource = new CancellationTokenSource();
        //设置CancelToken
        sp.GetRequiredService<AsyncContextService<CancellationTokenSource>>().Set(tokenSource);

        //执行请求
        try
        {
            var req = await bindService.BindAsync(apiType);
            //验证DTO
            if (req is IReqValidator { } validator && validator.Validate() is { IsValid: false } vresult)
            {
                return TypedResults.ValidationProblem(vresult.ToDictionary());
            }

            var api = sp.GetRequiredService(apiType);

            var result = await ((dynamic)api)!.ExecuteAsync(req!, tokenSource.Token);
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
            if (sp.GetService<IQuickApiExceptionResultBuilder>() is { } exceptionResultBuilder)
            {
                return await exceptionResultBuilder.ErrorResultAsync(ex);
            }
            //默认使用ProblemDetails
            throw;
        }
        finally
        {
            tokenSource.Dispose();
        }

        /// <summary>
        /// 内部返回的Result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        static IResult InnerResult(object? result) => result switch
        {
            null => Results.Ok(),
            IResult iresult => iresult,
#pragma warning disable CS0618
            IResultResponse iresult => iresult.Result,
#pragma warning restore CS0618
            string iresult => Results.Content(iresult),
            _ => Results.Json(result),
        };
    }
}