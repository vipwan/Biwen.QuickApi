
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
    public static RouteGroupBuilder MapXGenQuickApis(this IEndpointRouteBuilder app, string prefix = "api")
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }
        var groupBuilder = app.MapGroup(prefix);

        groupBuilder.WithMetadata(new QuickApiMetadata(null));
        //metadata
        groupBuilder.WithMetadata(new QuickApiMetadata(typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Test6)));

        //outputcache
        var outputCacheAttribute = typeof(Biwen.QuickApi.SourceGenerator.TestConsole.Test6).GetCustomAttribute<OutputCacheAttribute>();
        if (outputCacheAttribute != null) groupBuilder.WithMetadata(outputCacheAttribute);
    

        return groupBuilder;
    }
}