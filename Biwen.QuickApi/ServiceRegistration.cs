
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;

namespace Biwen.QuickApi
{

    public static class ServiceRegistration
    {

        /// <summary>
        /// Add Biwen.QuickApis
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBiwenQuickApis(this IServiceCollection services)
        {
            //注册验证器
            services.AddFluentValidationAutoValidation();
            services.AddHttpContextAccessor();
            services.AddMemoryCache();

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

            //注册Api
            var apis = ASS.InAllRequiredAssemblies.Where(x => !x.IsAbstract && x.IsClass && x.IsAssignableTo(typeof(IQuickApi)));
            foreach (var api in apis)
            {
                services.AddScoped(api);
            }
            return services;
        }

        /// <summary>
        /// 拆分枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        private static List<T> SplitEnum<T>(this T e) where T : Enum
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


        /// <summary>
        /// Map Biwen.QuickApis
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="QuickApiExcetion"></exception>
        public static (string, RouteGroupBuilder)[] MapBiwenQuickApis(this IEndpointRouteBuilder app)
        {
            var apis = ASS.InAllRequiredAssemblies.Where(x => !x.IsAbstract && x.IsClass && x.IsAssignableTo(typeof(IQuickApi)));
            if (apis.Any(x => x.GetCustomAttribute<QuickApiAttribute>() == null))
            {
                throw new QuickApiExcetion($"所有QuickApi都必须标注QuickApi特性!");
            }
            //分组:
            var groups = apis.GroupBy(x => x.GetCustomAttribute<QuickApiAttribute>()!.Group.ToLower());
            var routeGroups = new List<(string, RouteGroupBuilder)>();
            foreach (var group in groups)
            {
                var g = app.MapGroup(group.Key);
                foreach (var api in group)
                {
                    var attr = api.GetCustomAttribute<QuickApiAttribute>() ?? throw new QuickApiExcetion($"{api.Name}:必须标注QuickApi特性!");
                    var verbs = attr.Verbs.SplitEnum();//拆分枚举
                    foreach (var verb in verbs)
                    {
                        RouteHandlerBuilder routeHandlerBuilder = null!;

                        switch (verb)
                        {
                            case Verb.GET:
                            default:
                                {
                                    routeHandlerBuilder = g.MapGet(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await RequestHandler(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.POST:
                                {
                                    routeHandlerBuilder = g.MapPost(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await RequestHandler(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.PUT:
                                {
                                    routeHandlerBuilder = g.MapPut(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await RequestHandler(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.DELETE:
                                {
                                    routeHandlerBuilder = g.MapDelete(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await RequestHandler(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.PATCH:
                                {
                                    routeHandlerBuilder = g.MapPatch(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await RequestHandler(ctx, api, attr);
                                    });
                                }
                                break;
                        }

                        //OpenApi 生成
                        var method = api.GetMethod("ExecuteAsync")!;
                        var parameter = method.GetParameters()[0]!;
                        var parameterType = parameter.ParameterType!;

                        if (verb != Verb.GET && parameterType != typeof(EmptyRequest))
                        {
                            routeHandlerBuilder!.Accepts(parameterType, "application/json");
                        }
                        //401
                        if (!string.IsNullOrEmpty(attr.Policy))
                        {
                            routeHandlerBuilder!.ProducesProblem(StatusCodes.Status401Unauthorized);
                        }
                        //200
                        var retnType = method.ReturnType.GenericTypeArguments[0];
                        routeHandlerBuilder!.Produces(200, retnType == typeof(EmptyResponse) ? null : retnType);
                        //400
                        if (parameterType != typeof(EmptyRequest))
                        {
                            routeHandlerBuilder!.ProducesValidationProblem();
                        }
                        //500
                        routeHandlerBuilder!.ProducesProblem(StatusCodes.Status500InternalServerError);
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
        /// <param name="api"></param>
        /// <param name="quickApiAttribute"></param>
        /// <returns></returns>
        /// <exception cref="QuickApiExcetion"></exception>
        async static Task<IResult> RequestHandler(IHttpContextAccessor ctx, Type api, QuickApiAttribute quickApiAttribute)
        {
            //验证策略
            var policy = quickApiAttribute.Policy;
            if (!string.IsNullOrEmpty(policy))
            {
                var httpContext = ctx.HttpContext;
                var authService = httpContext!.RequestServices.GetRequiredService<IAuthorizationService>();
                var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                if (!authorizationResult.Succeeded)
                {
                    return Results.Unauthorized();
                }
            }

            object? o = ctx.HttpContext!.RequestServices.GetRequiredService(api);
            var cache = ctx.HttpContext!.RequestServices.GetRequiredService<IMemoryCache>();

            var method = api.GetMethod("ExecuteAsync")!;
            var parameter = method.GetParameters()[0]!;
            //var fromAttr = parameter.GetCustomAttribute<FromAttribute>();
            var parameterType = parameter.ParameterType!;
            //使用缓存,提升性能
            object? req = await cache.GetOrCreateAsync($"biwen.quickapi.{parameterType.FullName}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365);
                return await Task.FromResult(Activator.CreateInstance(parameterType)!);
            });

            //获取请求对象
            var bindMethod = parameterType.BaseType!.GetMethod("BindAsync")!;
            dynamic bindRutn = bindMethod.Invoke(req, new object[] { ctx.HttpContext })!;
            req = bindRutn.Result;

            //验证DTO
            (bool, IDictionary<string, string[]>?) Valid(MethodInfo? md, object validator)
            {
                //验证不通过的情况
                if (md!.Invoke(validator, new[] { req }) is ValidationResult result && !result!.IsValid)
                {
                    return (false, result.ToDictionary());
                }
                return (true, null);
            }
            if (req != null)
            {
                //继承至ValidationSettingBase<T>的情况
                if (o.GetType().BaseType!.IsConstructedGenericType && o.GetType().BaseType!.GenericTypeArguments.Any(x => x == req!.GetType()))
                {
                    var x = req as IRequestValidator ?? throw new QuickApiExcetion($"IRequestValidator is Null!");
                    var md = x.RealValidator.GetType().GetMethods().First(
                        x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                    //验证不通过的情况
                    var vResult = Valid(md, x.RealValidator);
                    if (!vResult.Item1)
                    {
                        return Results.ValidationProblem(vResult.Item2!);
                    }
                }
            }

            //使用异步方法
            var result = ((dynamic)method.Invoke(o, new object[] { req! })!).Result;

            if (result is EmptyResponse)
            {
                return Results.Ok();//返回空
            }
            return Results.Ok(result);
        }
    }
}