
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
using Biwen.QuickApi.Http;
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
    /// <param name=""app""></param>
    /// <param name=""serviceProvider""></param>
    /// <param name=""prefix"">全局路由前缀</param>
    /// <returns></returns>
    /// <exception cref=""ArgumentNullException""></exception>
    /// <exception cref=""QuickApiExcetion""></exception>
    public static (string Group, RouteGroupBuilder RouteGroupBuilder)[] MapGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider,string prefix = "api")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }
        //middleware:
        (app as WebApplication)?.UseMiddleware<QuickApiMiddleware>();
        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();
        List<(string, RouteGroupBuilder)> groups = new();
        //step1
        {
            //step2
            //group: hello
            var group4a35a790 = GroupBuilder(groupBuilder, serviceProvider, "hello");
            {
                //step3
                var map8e0e61a8 = group4a35a790.MapMethods("test1", new[] { "GET","POST" }, async (IHttpContextAccessor ctx, Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi api) =>
                {
                    var checkResult = await CheckPolicy(ctx, "admin");
                    if (!checkResult.Flag) return checkResult.Result!;
                    var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);
                    var vresult = req.Validate();
                    if (!vresult.IsValid) { return TypedResults.ValidationProblem(vresult.ToDictionary()); }
                    try
                    {
                        var result = await api.ExecuteAsync(req!);
                        var resultFlag = InnerResult(result);
                        if (resultFlag.Flag) return resultFlag.Result!;
                        return TypedResults.Json(result);
                    }
                    catch (Exception ex)
                    {
                        var exceptionHandlers = ctx.HttpContext!.RequestServices.GetServices<IQuickApiExceptionHandler>();
                        foreach (var handler in exceptionHandlers)
                        {
                            await handler.HandleAsync(ex);
                        }
                        var exceptionResultBuilder = ctx.HttpContext!.RequestServices.GetRequiredService<IQuickApiExceptionResultBuilder>();
                        return await exceptionResultBuilder.ErrorResult(ex);
                    }
                });
                //metadata
                map8e0e61a8.WithMetadata(new QuickApiMetadata(typeof(Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi)));
                //handlerbuilder
                scope.ServiceProvider.GetRequiredService<Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi>().HandlerBuilder(map8e0e61a8);
                //outputcache
                var map8e0e61a8Cache = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi).GetCustomAttribute<OutputCacheAttribute>();
                if (map8e0e61a8Cache != null) map8e0e61a8.WithMetadata(map8e0e61a8Cache);
                //endpointgroup
                var map8e0e61a8EndpointgroupAttribute = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi).GetCustomAttribute<EndpointGroupNameAttribute>();
                if (map8e0e61a8EndpointgroupAttribute != null) map8e0e61a8.WithMetadata(map8e0e61a8EndpointgroupAttribute);
                //authorizeattribute
                var map8e0e61a8authorizeAttributes = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi).GetCustomAttributes<AuthorizeAttribute>();
                if (map8e0e61a8authorizeAttributes.Any()) map8e0e61a8.WithMetadata(new AuthorizeAttribute());
                foreach (var authAttr in map8e0e61a8authorizeAttributes)
                {
                    map8e0e61a8.WithMetadata(authAttr);
                }
                //allowanonymous
                var map8e0e61a8allowanonymous = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.TestQuickApi).GetCustomAttribute<AllowAnonymousAttribute>();
                if (map8e0e61a8allowanonymous != null) map8e0e61a8.WithMetadata(map8e0e61a8allowanonymous);


            }
            groups.Add(("hello", group4a35a790));
        }


        return groups.ToArray();
    }

    /// <summary>
    /// GroupBuilder
    /// </summary>
    /// <returns></returns>
    private static RouteGroupBuilder GroupBuilder(RouteGroupBuilder groupBuilder, IServiceProvider serviceProvider, string group)
    {
        if (group == null) { return groupBuilder; }
        groupBuilder = groupBuilder.MapGroup(group);

        //GroupRouteBuilder
        var groupRouteBuilders = serviceProvider.GetServices<IQuickApiGroupRouteBuilder>();
        foreach (var groupRouteBuilder in groupRouteBuilders.OrderBy(x => x.Order))
        {
            if (groupRouteBuilder.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
            {
                groupBuilder = groupRouteBuilder.Builder(groupBuilder);
            }
        }
        return groupBuilder;
    }

    /// <summary>
    /// 验证Policy
    /// </summary>
    /// <exception cref=""QuickApiExcetion""></exception>
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
        if (result is EmptyResponse)
        {
            return (true, TypedResults.Ok());
        }
        if (result is ContentResponse content)
        {
            return (true, TypedResults.Content(content.ToString()));
        }
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


