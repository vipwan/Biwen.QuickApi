
## QuickApi源代码生成器

### 介绍
- Biwen.QuickApi.SourceGenerator 1.0.0已经发布欢迎使用.
- 提供预生成IEndpointRouteBuilder的扩展方法,用于快速注册Api.用于显著提升性能和开发效率.

### 说明
- 
- 由于1.0版本使用Emit和动态类型会导致性能上的损失.所以提供SourceGenerator源代码生成器的方式生成强类型代码.

### Enjoy!

- 1.安装Biwen.QuickApi.SourceGenerator 1.1.0
- <del>2.全局引用你[QuickApi]特性标注的地方 比如: global using Biwen.QuickApi.DemoWeb.Apis;</del>
- 3.调用 app.MapGenQuickApis("api"); 注册所有Api,尽情享用吧!


### SourceGenerator生成代码样例
```csharp

//code gen for Biwen.QuickApi
//version :1.0.0.0
//author :vipwan@outlook.com 万雅虎
//https://github.com/vipwan/Biwen.QuickApi
//如果你在使用中遇到问题,请第一时间issue,谢谢!

using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

using Biwen.QuickApi.SourceGenerator.TestConsole.Api2;
using Biwen.QuickApi.SourceGenerator.TestConsole;

namespace Biwen.QuickApi;

#pragma warning disable
public static partial class AppExtentions
{

    /// <summary>
    /// 注册所有的QuickApi(Gen版)
    /// </summary>
    /// <param name="app"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="prefix">全局路由前缀</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="QuickApiExcetion"></exception>
    public static RouteGroupBuilder MapGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider,string prefix = "api")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        //middleware:
        (app as WebApplication)?.UseMiddleware<QuickApiMiddleware>();

        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();
        
                var map1aa5c30c = groupBuilder.MapMethods("/test6", new[] { "GET" }, async (IHttpContextAccessor ctx, Biwen.QuickApi.SourceGenerator.TestConsole.Test6 api) =>
            {
                //验证策略
                var checkResult = await CheckPolicy(ctx, "");
                if (!checkResult.Flag) return checkResult.Result!;
                //绑定对象
                var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);
                //验证器
                if (req.RealValidator.Validate(req) is ValidationResult vresult && !vresult!.IsValid)
                {
                    return TypedResults.ValidationProblem(vresult.ToDictionary());
                }
                //执行请求
                try
                {
                    var result = await api.ExecuteAsync(req!);
#pragma warning disable CS0184 // '"is" 表达式的给定表达式始终不是所提供的类型
                    var resultFlag = InnerResult(result);
                    if (resultFlag.Flag) return resultFlag.Result!;
#pragma warning restore CS0184 // '"is" 表达式的给定表达式始终不是所提供的类型
                    return TypedResults.Json(result);
                }
                catch (Exception ex)
                {
                    var exceptionHandlers = ctx.HttpContext!.RequestServices.GetServices<IQuickApiExceptionHandler>();
                    //异常处理
                    foreach (var handler in exceptionHandlers)
                    {
                        await handler.HandleAsync(ex);
                    }
                    //规范化异常返回
                    var exceptionResultBuilder = ctx.HttpContext!.RequestServices.GetRequiredService<IQuickApiExceptionResultBuilder>();
                    return await exceptionResultBuilder.ErrorResult(ex);
                }
            });
        
        //metadata
        map1aa5c30c.WithMetadata(new QuickApiMetadata(typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Test6)));
        //handler
        scope.ServiceProvider.GetRequiredService<Biwen.QuickApi.SourceGenerator.TestConsole.Test6>().HandlerBuilder(map1aa5c30c);
        //outputcache
        var map1aa5c30cCache = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Test6).GetCustomAttribute<OutputCacheAttribute>();
        if (map1aa5c30cCache != null) map1aa5c30c.WithMetadata(map1aa5c30cCache);
        //endpointgroup
        var map1aa5c30cEndpointgroupAttribute = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Test6).GetCustomAttribute<EndpointGroupNameAttribute>();
        if (map1aa5c30cEndpointgroupAttribute != null) map1aa5c30c.WithMetadata(map1aa5c30cEndpointgroupAttribute);
        
        return groupBuilder;
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
    static (bool Flag, IResult? Result) InnerResult(object? result)
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
#pragma warning restore


```

### 参考文档
- https://learn.microsoft.com/zh-cn/dotnet/csharp/roslyn-sdk/source-generators-overview
- https://github.com/dotnet/roslyn-sdk/tree/main/samples/CSharp/SourceGenerators

- https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#out-of-scope-designs



### 如何调试Generator
- [如何调试](https://github.com/JoanComasFdz/dotnet-how-to-debug-source-generator-vs2022#solution-structure)


