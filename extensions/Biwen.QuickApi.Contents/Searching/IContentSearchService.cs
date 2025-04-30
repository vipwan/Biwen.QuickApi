// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-29 14:45:02 IContentSearchService.cs

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.UnitOfWork.Pagenation;

namespace Biwen.QuickApi.Contents.Searching;

/// <summary>
/// 内容搜索服务
/// </summary>
public interface IContentSearchService
{
    /// <summary>
    /// 健康检测
    /// </summary>
    /// <returns></returns>
    Task<bool> HealthCheckAsync();


    /// <summary>
    /// 初始化索引
    /// </summary>
    /// <returns></returns>
    Task InitializeIndexAsync();

    /// <summary>
    /// 重新构建索引
    /// </summary>
    /// <param name="allContents"></param>
    /// <returns></returns>
    Task RebuildIndexAsync(IEnumerable<Content> allContents);


    Task AddOrUpdateDocumentAsync(Content blog);


    Task DeleteDocumentAsync(Guid id);

    /// <summary>
    /// 查询文档
    /// </summary>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="filter"></param>
    /// <param name="sort"></param>
    /// <param name="enableHighlight"></param>
    /// <param name="facets"></param>
    /// <returns></returns>
    Task<IPagedList<ContentSearchResult>> SearchContentsAsync(
    string query,
    int page = 1,
    int pageSize = 10,
    string? filter = null,
    string? sort = null,
    bool enableHighlight = true,
    string[]? facets = null
    );

}
