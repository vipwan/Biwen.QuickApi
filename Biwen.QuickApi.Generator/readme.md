
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
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Microsoft.Extensions.DependencyInjection;

using Biwen.QuickApi.SourceGenerator.TestConsole;

namespace Biwen.QuickApi;

#pragma warning disable
public static partial class AppExtentions
{

    /// <summary>
    /// 注册所有的QuickApi(Gen版)
    /// </summary>
    /// <param name="app"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="QuickApiExcetion"></exception>
    public static RouteGroupBuilder MapGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider,string prefix = "api")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }
        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();
        
        var mapTestQuickApi = groupBuilder.MapMethods("hello/test1", new[] { "GET","POST" }, async (IHttpContextAccessor ctx, TestQuickApi api) =>
            {
                //验证策略
                var policy = "admin";
                if (!string.IsNullOrEmpty(policy))
                {
                    var httpContext = ctx.HttpContext;
                    var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null,besure services.AddAuthorization() first!");
                    var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                    if (!authorizationResult.Succeeded)
                    {
                        return TypedResults.Unauthorized();
                    }
                }
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
                    if(result is EmptyResponse)
                    {
                        return TypedResults.Ok();
                    }
                    if(result is ContentResponse)
                    {
                        return Results.Content(result.ToString());
                    }
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
                    //默认处理
                    throw;
                }
            });
        //handler
        scope.ServiceProvider.GetRequiredService<TestQuickApi>().HandlerBuilder(mapTestQuickApi);

        return groupBuilder;
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


