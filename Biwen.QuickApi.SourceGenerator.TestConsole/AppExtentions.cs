using Biwen.QuickApi.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Biwen.QuickApi.SourceGenerator.TestConsole
{
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
        public static RouteGroupBuilder MapGenQuickApis(this IEndpointRouteBuilder app, string prefix = "api")
        {
            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            var groupBuilder = app.MapGroup(prefix);

            groupBuilder.MapMethods("$0", new[] { "GET","POST" }, async (IHttpContextAccessor ctx, TestPostQuickApi api) =>
            {

                //验证策略
                var policy = "$2";
                if (!string.IsNullOrEmpty(policy))
                {
                    var httpContext = ctx.HttpContext;
                    var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null,besure services.AddAuthorization() first!");
                    var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                    if (!authorizationResult.Succeeded)
                    {
                        return Results.Unauthorized();
                    }
                }
                //绑定对象
                var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);

                //验证器
                if (req.RealValidator.Validate(req) is ValidationResult vresult && !vresult!.IsValid)
                {
                    return Results.ValidationProblem(vresult.ToDictionary());
                }

                //执行请求
                try
                {
                    var result = await api.ExecuteAsync(req!);
                    return Results.Json(result);
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


            groupBuilder.MapGet("$1", async (IHttpContextAccessor ctx, TestPostQuickApi api) =>
            {

                //验证策略
                var policy = "$2";
                if (!string.IsNullOrEmpty(policy))
                {
                    var httpContext = ctx.HttpContext;
                    var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ??
                        throw new QuickApiExcetion($"IAuthorizationService is null,besure services.AddAuthorization() first!");
                    var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
                    if (!authorizationResult.Succeeded)
                    {
                        return Results.Unauthorized();
                    }
                }
                //绑定对象
                var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);

                //验证器
                if (req.RealValidator.Validate(req) is ValidationResult vresult && !vresult!.IsValid)
                {
                    return Results.ValidationProblem(vresult.ToDictionary());
                }
                var quickApiOptions = ctx.HttpContext!.RequestServices.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value;

                //执行请求
                try
                {
                    var result = await api.ExecuteAsync(req!);
                    return Results.Json(result);
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

            return groupBuilder;
        }


    }
}