// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-10 13:56:48 IDocumentRenderService.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;

namespace Biwen.QuickApi.Contents.Rendering;

/// <summary>
/// 视图渲染服务,默认实现为RazorDocumentRenderService,可以根据需求自定义实现
/// </summary>
public interface IDocumentRenderService
{
    /// <summary>
    /// 渲染文档
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    ///Task<string> RenderDocumentAsync<T>(T document, Content content) where T : class, IContent;


    /// <summary>
    /// 根据文档ID渲染文档
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<string> RenderDocumentAsync(Guid id);

    /// <summary>
    /// 根据文档slug渲染文档
    /// </summary>
    /// <param name="slug">文档的唯一标识符</param>
    /// <returns>渲染后的HTML</returns>
    Task<string> RenderDocumentBySlugAsync(string slug);

}