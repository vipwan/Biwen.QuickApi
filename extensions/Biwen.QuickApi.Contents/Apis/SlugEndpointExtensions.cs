// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-10 18:33:32 SlugEndpointExtensions.cs

using Biwen.QuickApi.Contents.Rendering;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi;

/// <summary>
/// 可以根据实际情况分文档类型生成.这里提供一个简单的实现
/// </summary>
public static class SlugEndpointExtensions
{
    /// <summary>
    /// 添加Slug路由
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="slug"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapBiwenContentsBySlug(this IEndpointRouteBuilder builder, string prefix = "contents")
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix), "前缀不能为空");
        }

        //可以根据实际情况分文档类型生成.这里提供一个简单的实现

        builder.MapGet("/{prefix}/{slug}", async (
            [FromRoute] string slug,
            [FromServices] IDocumentRenderService documentRenderService) =>
        {
            var content = await documentRenderService.RenderDocumentBySlugAsync(slug);
            if (string.IsNullOrEmpty(content))
            {
                return Results.NotFound();
            }
            return Results.Content(content, "text/html");
        }).ExcludeFromDescription();

        return builder;
    }
}