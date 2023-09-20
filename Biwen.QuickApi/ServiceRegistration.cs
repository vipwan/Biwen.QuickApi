using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi
{

    public static class ServiceRegistration
    {
        public static IServiceCollection AddBiwenQuickApis(this IServiceCollection services)
        {
            //注册验证器
            services.AddFluentValidationAutoValidation();
            services.AddHttpContextAccessor();

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
            //services.Scan(scan =>
            //{
            //    scan.FromAssemblies(ASS.AllRequiredAssemblies).AddClasses(x =>
            //    {
            //        x.AssignableTo(typeof(IQuickApi));//来自指定的接口
            //        x.Where(a => { return a.IsClass && !a.IsAbstract && !a.IsGenericTypeDefinition; });
            //    })
            //    .AsSelf()//Self
            //    .AsImplementedInterfaces(x => x.IsGenericType) //实现基于他的接口
            //    .WithTransientLifetime();  //AddTransient
            //});

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
                        switch (verb)
                        {
                            case Verb.GET:
                            default:
                                {
                                    g.MapGet(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await @delegate(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.POST:
                                {
                                    g.MapPost(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await @delegate(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.PUT:
                                {
                                    g.MapPut(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await @delegate(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.DELETE:
                                {
                                    g.MapDelete(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await @delegate(ctx, api, attr);
                                    });
                                }
                                break;
                            case Verb.PATCH:
                                {
                                    g.MapPatch(attr.Route, async (IHttpContextAccessor ctx) =>
                                    {
                                        return await @delegate(ctx, api, attr);
                                    });
                                }
                                break;
                        }
                    }
                }
                routeGroups.Add((group.Key, g));
            }
            async Task<IResult> @delegate(IHttpContextAccessor ctx, Type api, QuickApiAttribute quickApiAttribute)
            {
                //验证策略
                var policy = quickApiAttribute.Policy;
                if (!string.IsNullOrEmpty(policy))
                {
                    var httpContext = ctx.HttpContext;
                    var authService = httpContext!.RequestServices.GetRequiredService<IAuthorizationService>();
                    var authorizationResult = authService.AuthorizeAsync(httpContext.User, policy).GetAwaiter().GetResult();
                    if (!authorizationResult.Succeeded)
                    {
                        return Results.Unauthorized();
                    }
                }

                var ctors = api.GetConstructors();
                if (ctors.Length == 0)
                {
                    throw new QuickApiExcetion($"{api.Name}必须具有构造函数!");
                }
                var ctor = ctors[0];
                object? o = null;
                if (ctor.GetParameters().Length == 0)
                {
                    o = Activator.CreateInstance(api)!;
                }
                else
                {
                    var ctorParams = ctor.GetParameters();
                    var ctorParamInstances = new List<object>();
                    foreach (var ctorParam in ctorParams)
                    {
                        var paramType = ctorParam.ParameterType;
                        var paramInstance = ctx.HttpContext!.RequestServices.GetService(paramType) ??
                            throw new QuickApiExcetion($"{paramType.Name}未注册!");
                        ctorParamInstances.Add(paramInstance);
                    }
                    o = Activator.CreateInstance(api, ctorParamInstances.ToArray())!;
                }
                o.GetType().GetProperty("HttpContextAccessor")!.SetValue(o, ctx);
                o.GetType().GetProperty("ServiceProvider")!.SetValue(o, ctx.HttpContext!.RequestServices);
                var method = api.GetMethod("Execute")!;
                var parameter = method.GetParameters()[0]!;
                var fromAttr = parameter.GetCustomAttribute<FromAttribute>();
                var parameterType = parameter.ParameterType!;
                object? req = null;
                if (fromAttr != null)
                {
                    switch (fromAttr.From)
                    {
                        case RequestFrom.FromQuery:
                        default:
                            {
                                var qs = ctx.HttpContext.Request.Query;
                                req = Activator.CreateInstance(parameterType)!;
                                foreach (var item in qs)
                                {
                                    var prop = parameterType.GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                                    if (prop != null)
                                    {
                                        //转换
                                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                                        prop.SetValue(req, value);
                                    }
                                }
                            }
                            break;
                        case RequestFrom.FromForm:
                            {
                                var qs = ctx.HttpContext.Request.Form;
                                req = Activator.CreateInstance(parameterType)!;
                                foreach (var item in qs)
                                {
                                    var prop = parameterType.GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                                    if (prop != null)
                                    {
                                        //转换
                                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                                        prop.SetValue(req, item.Value);
                                    }
                                }
                            }
                            break;
                        case RequestFrom.FromBody:
                            req = await ctx.HttpContext.Request.ReadFromJsonAsync(parameterType);
                            break;
                        case RequestFrom.FromRoute:
                            {
                                var qs = ctx.HttpContext.Request.RouteValues;
                                req = Activator.CreateInstance(parameterType)!;
                                foreach (var item in qs)
                                {
                                    var prop = parameterType.GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                                    if (prop != null)
                                    {
                                        //转换
                                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFrom(item.Value!);
                                        prop.SetValue(req, item.Value);
                                    }
                                }
                            }
                            break;
                        case RequestFrom.FromHead:
                            {
                                var qs = ctx.HttpContext.Request.Headers;
                                req = Activator.CreateInstance(parameterType)!;
                                foreach (var item in qs)
                                {
                                    var prop = parameterType.GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                                    if (prop != null)
                                    {
                                        //转换
                                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                                        prop.SetValue(req, item.Value);
                                    }
                                }
                            }
                            break;
                    }
                }
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
                var result = method.Invoke(o, new object[] { req! });

                if (result is EmptyResponse)
                {
                    return Results.Ok();//返回空
                }
                return Results.Ok(result);
            }
            return routeGroups.ToArray();
        }
    }
}