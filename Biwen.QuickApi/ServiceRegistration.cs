
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Dynamic;

namespace Biwen.QuickApi
{
    public static class ServiceRegistration
    {

        /// <summary>
        /// Add Biwen.QuickApis
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
#pragma warning disable CS0618 // 类型或成员已过时
        public static IServiceCollection AddBiwenQuickApis(this IServiceCollection services, Action<BiwenQuickApiOptions>? options = null)
#pragma warning restore CS0618 // 类型或成员已过时
        {
            //注册验证器
            services.AddFluentValidationAutoValidation();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

            services.AddProblemDetails();

            //options
#pragma warning disable CS0618 // 类型或成员已过时
            services.AddOptions<BiwenQuickApiOptions>().Configure(o => { options?.Invoke(o); });
#pragma warning restore CS0618 // 类型或成员已过时

            //services.Scan(scan =>
            //{
            //    scan.FromAssemblies(ASS.AllRequiredAssemblies).AddClasses(x =>
            //    {
            //        x.AssignableTo(typeof(IValidator<>));//来自指定的接口
            //                                             //必须是类,且当前Class不是泛型类.排除ValidationSettingBase<T>,且不为抽象类
            //        x.Where(a => { return a.IsClass && !a.IsAbstract && !a.IsGenericTypeDefinition; });
            //    })
            //    .AsImplementedInterfaces(x => x.IsGenericType) //实现基于他的接口
            //    .WithTransientLifetime();  //AddTransient
            //});

            foreach (var api in Apis)
            {
                //注册Api
                services.AddScoped(api);
            }

            //foreach (var binder in Binders)
            //{
            //    //注册ReqBinder
            //    services.AddSingleton(binder);
            //}

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
            //分组:
            var groups = Apis.GroupBy(x => x.GetCustomAttribute<QuickApiAttribute>()!.Group.ToLower());
            var routeGroups = new List<(string, RouteGroupBuilder)>();
            //quickapi前缀
#pragma warning disable CS0618 // 类型或成员已过时
            var prefix = app.ServiceProvider.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value.RoutePrefix;
#pragma warning restore CS0618 // 类型或成员已过时
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
                foreach (var apiType in group)
                {
                    var attr = apiType.GetCustomAttribute<QuickApiAttribute>() ?? throw new QuickApiExcetion($"{apiType.Name}:必须标注QuickApi特性!");

                    if (apiType.GetCustomAttribute<JustAsServiceAttribute>() != null)
                    {
                        //不需要注册路由的QuickApi
                        continue;
                    }
                    var verbs = attr.Verbs.SplitEnum();//拆分枚举
                    foreach (var verb in verbs)
                    {
                        RouteHandlerBuilder? rhBuilder = null!;

                        //MapMethods
                        rhBuilder = g.MapMethods(attr.Route, new[] { verb.ToString() }, async (IHttpContextAccessor ctx) =>
                        {
                            return await RequestHandler(ctx, apiType, attr);
                        });

                        //HandlerBuilder
                        using var scope = app.ServiceProvider.CreateAsyncScope();
                        var currentApi = scope.ServiceProvider.GetRequiredService(apiType);
                        if (currentApi is IHandlerBuilder hb)
                        {
                            rhBuilder = hb.HandlerBuilder(rhBuilder!);
                        }

                        //OpenApi 生成
                        //var method = apiType.GetMethod("ExecuteAsync")!;
                        //var parameter = method.GetParameters()[0]!;
                        //var parameterType = parameter.ParameterType!;
                        var parameterType = ((dynamic)currentApi).ReqType as Type;

                        if (verb != Verb.GET && parameterType != typeof(EmptyRequest))
                        {
                            rhBuilder!.Accepts(parameterType!, "application/json");
                        }
                        //401
                        if (!string.IsNullOrEmpty(attr.Policy))
                        {
                            rhBuilder?.ProducesProblem(StatusCodes.Status401Unauthorized);
                        }
                        ////200
                        ////var retnType = method.ReturnType.GenericTypeArguments[0];
                        //var retnType = ((dynamic)currentApi).RspType as Type;
                        //if (retnType == typeof(ContentResponse))
                        //{
                        //    rhBuilder?.Produces(200, typeof(string), contentType: "text/plain");
                        //}
                        //else
                        //{
                        //    rhBuilder?.Produces(200, retnType == typeof(EmptyResponse) ? null : retnType);
                        //}
                        ////400
                        //if (parameterType != typeof(EmptyRequest))
                        //{
                        //    rhBuilder?.ProducesValidationProblem();
                        //}
                        ////500
                        //rhBuilder?.ProducesProblem(StatusCodes.Status500InternalServerError);
                    }
                }
                routeGroups.Add((group.Key, g));
            }

            return routeGroups.ToArray();
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
            var policy = quickApiAttribute.Policy;
            if (!string.IsNullOrEmpty(policy))
            {
                var httpContext = ctx.HttpContext;
                var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ??
                    throw new QuickApiExcetion($"IAuthorizationService is null,besure services.AddAuthorization() first!");
                var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                if (!authorizationResult.Succeeded)
                {
                    return TypedResults.Unauthorized();
                }
            }
            object? api = ctx.HttpContext!.RequestServices.GetRequiredService(apiType);

#pragma warning disable CS0618 // 类型或成员已过时
            var quickApiOptions = ctx.HttpContext!.RequestServices.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value;
#pragma warning restore CS0618 // 类型或成员已过时


            //var cache = ctx.HttpContext!.RequestServices.GetRequiredService<IMemoryCache>();
            //var method = apiType.GetMethod("ExecuteAsync")!;
            //var parameter = method.GetParameters()[0]!;
            //var parameterType = parameter.ParameterType!;

            //所有的QuickApi都实现了IQuickApi,因此ReqType不可能为Null
            //var parameterType = ((dynamic)api).ReqType as Type;

            //使用缓存,提升性能
            //object? req = await cache.GetOrCreateAsync($"biwen.quickapi.{parameterType.FullName}", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365);
            //    return await Task.FromResult(Activator.CreateInstance(parameterType)!);
            //});

            //var bindMethod = parameterType.BaseType!.GetMethod("BindAsync")!;
            //dynamic bindRutn = bindMethod.Invoke(req, new object[] { ctx.HttpContext })!;
            //req = bindRutn.Result;

            //获取请求对象,使用dynamic代替反射
            //约定:所有的请求对象都实现了IReqBinder,因此不可能为Null
            //var reqBinder = ctx.HttpContext!.RequestServices.GetRequiredService(parameterType!);
            //var req = await ((dynamic)reqBinder!).BindAsync(ctx.HttpContext!);

            var req = await ((dynamic)api).ReqBinder.BindAsync(ctx.HttpContext!);

            //验证DTO
            if (req != null)
            {
                //验证器
                if (req.RealValidator.Validate(req) is ValidationResult vresult && !vresult!.IsValid)
                {
                    return TypedResults.ValidationProblem(vresult.ToDictionary());
                }

                //(bool, IDictionary<string, string[]>?) Valid(MethodInfo? md, object validator)
                //{
                //    //验证不通过的情况
                //    if (md!.Invoke(validator, new[] { req }) is ValidationResult result && !result!.IsValid)
                //    {
                //        return (false, result.ToDictionary());
                //    }
                //    return (true, null);
                //}

                //继承至ValidationSettingBase<T>的情况
                //if (api.GetType().BaseType!.IsConstructedGenericType && api.GetType().BaseType!.GenericTypeArguments.Any(x => x == req!.GetType()))
                //{
                //    var x = req as IRequestValidator ?? throw new QuickApiExcetion($"IRequestValidator is Null!");
                //    var md = x.RealValidator.GetType().GetMethods().First(
                //        x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                //    //验证不通过的情况
                //    var vResult = Valid(md, x.RealValidator);
                //    if (!vResult.Item1)
                //    {
                //        return Results.ValidationProblem(vResult.Item2!);
                //    }
                //}
            }

            //执行请求
            try
            {
                //var result = await method.Invoke(api, new object[] { req! })!;
                var result = await ((dynamic)api)!.ExecuteAsync(req!);
                //返回空结果
                if (result is EmptyResponse)
                {
                    return TypedResults.Ok();//返回空
                }
                //返回文本结果
                if (result is ContentResponse content)
                {
                    return TypedResults.Content(content.ToString());
                }

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
                        var alias = prop.GetCustomAttribute<AliasAsAttribute>();
                        ((IDictionary<string, object>)expandoObject)[alias != null ? alias.Name : prop.Name] = prop.GetValue(rsp);
                    }

                    return expandoObject;
                };

                //返回JSON
                var expandoResult = rspToExpandoObject(result);
                //return Results.Json(expandoResult, quickApiOptions?.JsonSerializerOptions);
                return TypedResults.Json(expandoResult, quickApiOptions?.JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                var exceptionHandlers = ctx.HttpContext!.RequestServices.GetServices<IQuickApiExceptionHandler>();
                //异常处理
                foreach (var handler in exceptionHandlers)
                {
                    await handler.HandleAsync(ex);
                }
                //默认处理
                throw;
            }
        }
    }
}