
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Metadata;

using Biwen.QuickApi.SourceGenerator.TestConsole;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OutputCaching;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Biwen.QuickApi.Http;

//用于测试生成器的代码

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:在命名空间中声明类型", Justification = "<挂起>")]
public static partial class AppExtentions
{

    /*
     * $api : className 
     * $request : requestClassName
     * $response : responseClassName
     * 
     * 
     * $0:group
     * $1:路由地址
     * $2:验证策略
     * 
     * 
     */


    /// <summary>
    /// 源代码生成器的模板代码
    /// </summary>
    /// <param name="app"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="QuickApiExcetion"></exception>
    public static (string Group, RouteGroupBuilder RouteGroupBuilder)[] MapXGenQuickApis(this IEndpointRouteBuilder app, IServiceProvider serviceProvider, string prefix = "api")
    {
        //step0
        if (prefix == null)
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        //middleware:
        (app as WebApplication)?.UseMiddleware<QuickApiMiddleware>();
        var groupBuilder = app.MapGroup(prefix);
        using var scope = serviceProvider.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value;
        if (options.EnableAntiForgeryTokens)
        {
            //middleware:
#if !NET8_0_OR_GREATER
            (app as WebApplication)?.UseMiddleware<QuickApiAntiforgeryMiddleware>();
#endif
#if NET8_0_OR_GREATER
        (app as WebApplication)?.UseAntiforgery();
#endif
        }

        List<(string, RouteGroupBuilder)> groups = new();
        //step1
        {
            //step2
            //group: admin
            var group123456 = GroupBuilder(groupBuilder, serviceProvider, "admin");
            {
                //step3
                var map3f9bcb4e = group123456.MapMethods("test1", new[] { "GET" }, async (IHttpContextAccessor ctx, Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi api) =>
                {
                    await Task.CompletedTask;
                    return Results.Ok();
                });
                //metadata
                map3f9bcb4e.WithMetadata(new QuickApiMetadata(typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi)));
                //handlerbuilder
                scope.ServiceProvider.GetRequiredService<Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi>().HandlerBuilder(map3f9bcb4e);
                //outputcache
                var map3f9bcb4eCache = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi).GetCustomAttribute<OutputCacheAttribute>();
                if (map3f9bcb4eCache != null) map3f9bcb4e.WithMetadata(map3f9bcb4eCache);
                //endpointgroup
                var map3f9bcb4eEndpointgroupAttribute = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi).GetCustomAttribute<EndpointGroupNameAttribute>();
                if (map3f9bcb4eEndpointgroupAttribute != null) map3f9bcb4e.WithMetadata(map3f9bcb4eEndpointgroupAttribute);
                //authorizeattribute
                var map3f9bcb4eauthorizeAttributes = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi).GetCustomAttributes<AuthorizeAttribute>();
                if (map3f9bcb4eauthorizeAttributes.Any()) map3f9bcb4e.WithMetadata(new AuthorizeAttribute());
                foreach (var authAttr in map3f9bcb4eauthorizeAttributes)
                {
                    map3f9bcb4e.WithMetadata(authAttr);
                }
                //allowanonymous
                var map3f9bcb4eallowanonymous = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Api2.TestQuickApi).GetCustomAttribute<AllowAnonymousAttribute>();
                if (map3f9bcb4eallowanonymous != null) map3f9bcb4e.WithMetadata(map3f9bcb4eallowanonymous);
            }
            groups.Add(("admin", group123456));
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:删除未使用的私有成员", Justification = "<挂起>")]
    async static Task<(bool Flag, IResult? Result)> CheckPolicy(IHttpContextAccessor ctx, string? policy)
    {
        {
            if (string.IsNullOrEmpty(policy))
            {
                {
                    return (true, null);
                }
            }
            if (!string.IsNullOrEmpty(policy))
            {
                {
                    var httpContext = ctx.HttpContext;
                    var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null, besure services.AddAuthorization() first!");
                    var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                    if (!authorizationResult.Succeeded)
                    {
                        {
                            return (false, TypedResults.Unauthorized());
                        }
                    }
                }
            }
            return (true, null);
        }
    }

    /// <summary>
    /// 内部返回的Result
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:删除未使用的私有成员", Justification = "<挂起>")]
    static (bool Flag, IResult? Result) InnerResult(object? result)
    {
        {
            if (result is EmptyResponse)
            {
                {
                    return (true, TypedResults.Ok());
                }
            }
            if (result is ContentResponse content)
            {
                {
                    return (true, TypedResults.Content(content.ToString()));
                }
            }
            if (result is IResultResponse iresult)
            {
                {
                    return (true, iresult.Result);
                }
            }
            return (false, null);
        }
    }


}