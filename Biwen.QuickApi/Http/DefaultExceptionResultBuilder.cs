// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:11 DefaultExceptionResultBuilder.cs

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Biwen.QuickApi.Http;

/// <summary>
/// 默认的异常结果构建器
/// </summary>
internal class DefaultExceptionResultBuilder : IQuickApiExceptionResultBuilder
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHostEnvironment _environment;
    private readonly IHttpContextAccessor _contextAccessor;
    public DefaultExceptionResultBuilder(
        IProblemDetailsService problemDetailsService,
        IHostEnvironment environment,
        IHttpContextAccessor contextAccessor)
    {
        _problemDetailsService = problemDetailsService;
        _environment = environment;
        _contextAccessor = contextAccessor;
    }

    public async Task<IResult> ErrorResultAsync(Exception exception)
    {
        //通过全局配置
        //if (_environment.IsProduction())
        //{
        //    return Results.Problem(exception.Message);
        //}

        Dictionary<string, object?> result = new();

        result.Add("Status", 500);
        result.Add("CurrentUser", _contextAccessor.HttpContext?.User?.Identity?.Name);
        result.Add("Exception", new
        {
            Message = exception.Message,
            StackTrace = exception.StackTrace,
        });
        result.Add("RequestPath", _contextAccessor.HttpContext?.Request?.Path.Value);
        result.Add("Method", _contextAccessor.HttpContext?.Request.Method);
        result.Add("QueryString", _contextAccessor.HttpContext?.Request.QueryString.Value);

        await Task.CompletedTask;
        return Results.Problem(exception.Message, extensions: result);
    }
}