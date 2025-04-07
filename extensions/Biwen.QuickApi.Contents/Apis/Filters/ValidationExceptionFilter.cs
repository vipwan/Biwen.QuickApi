// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 23:03:27 AuthFilter.cs

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Biwen.QuickApi.Contents.Apis.Filters;

/// <summary>
/// 验证异常过滤器，用于拦截ValidationException并返回标准的ValidationProblem响应
/// </summary>
public class ValidationExceptionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            // 尝试执行下一个过滤器或API处理程序
            return await next(context);
        }
        catch (ValidationException ex)
        {
            // 处理验证异常
            var errors = new Dictionary<string, string[]>();

            // 如果ValidationException包含MemberNames信息，则将错误信息映射到相应成员
            if (ex.ValidationResult?.MemberNames?.Any() == true)
            {
                foreach (var memberName in ex.ValidationResult.MemberNames)
                {
                    errors[memberName] = [ex.ValidationResult.ErrorMessage ?? "验证失败"];
                }
            }
            else
            {
                // 否则使用Content作为默认键
                errors["Content"] = [ex.Message];
            }

            // 返回ValidationProblem结果
            return TypedResults.ValidationProblem(errors);
        }
    }
}
