
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;

using Biwen.QuickApi.SourceGenerator.TestConsole;
using Microsoft.OpenApi.Models;

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
    public static RouteGroupBuilder MapXGenQuickApis(this IEndpointRouteBuilder app, string prefix = "api")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }
        var groupBuilder = app.MapGroup(prefix);
        return groupBuilder;
    }
}