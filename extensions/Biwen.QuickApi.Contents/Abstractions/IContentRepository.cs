// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 22:12:38 IContentRepository.cs

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.UnitOfWork.Pagenation;

namespace Biwen.QuickApi.Contents.Abstractions;

/// <summary>
/// 内容仓储接口
/// </summary>
public interface IContentRepository
{
    /// <summary>
    /// 保存内容
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="content">内容对象</param>
    /// <param name="title">标题</param>
    /// <param name="slug">别名</param>
    /// <returns>内容ID</returns>
    Task<Guid> SaveContentAsync<T>(T content, string? title = null, string? slug = null) where T : IContent;

    /// <summary>
    /// 获取内容
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="id">内容ID</param>
    /// <returns>内容对象</returns>
    Task<T?> GetContentAsync<T>(Guid id) where T : IContent, new();

    /// <summary>
    /// 获取内容列表
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="pageIndex">页码</param>
    /// <param name="len">每页数量</param>
    /// <returns>分页内容列表</returns>
    Task<IPagedList<T>> GetContentsByTypeAsync<T>(int pageIndex = 0, int len = 10) where T : IContent, new();

    /// <summary>
    /// 获取内容列表
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="slug">别名</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="len">每页数量</param>
    /// <param name="status">状态</param>
    /// <param name="title">标题</param>
    /// <returns>分页内容列表</returns>
    Task<IPagedList<Content>> GetDomainContentsByTypeAsync<T>(
        string? slug = null,
        int pageIndex = 0,
        int len = 10,
        int? status = null,
        string? title = null
    ) where T : IContent, new();

    /// <summary>
    /// 根据别名获取内容
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="slug">别名</param>
    /// <returns>内容对象</returns>
    Task<T?> GetContentsByTypeAsync<T>(string slug) where T : IContent, new();

    /// <summary>
    /// 更新内容
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="id">内容ID</param>
    /// <param name="content">内容对象</param>
    Task UpdateContentAsync<T>(Guid id, T content) where T : IContent;

    /// <summary>
    /// 删除内容
    /// </summary>
    /// <param name="id">内容ID</param>
    Task DeleteContentAsync(Guid id);

    /// <summary>
    /// 设置内容状态
    /// </summary>
    /// <param name="id">内容ID</param>
    /// <param name="status">状态</param>
    Task SetContentStatusAsync(Guid id, ContentStatus status);

    /// <summary>
    /// 获取内容Schema
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <returns>Schema JSON</returns>
    string GetContentSchema<T>() where T : IContent;

    /// <summary>
    /// 获取原始内容实体
    /// </summary>
    /// <param name="id">内容ID</param>
    /// <returns>内容实体</returns>
    Task<Content> GetRawContentAsync(Guid id);


    /// <summary>
    /// 根据slug获取返回的第一个Id
    /// </summary>
    /// <param name="slug"></param>
    /// <returns></returns>
    Task<Content?> GetContentIdBySlugAsync(string slug);




    /// <summary>
    /// 更新原始内容实体
    /// </summary>
    /// <param name="content">内容实体</param>
    /// <param name="isRollback">是否回滚</param>
    /// <param name="version">版本</param>
    Task UpdateRawContentAsync(Content content, bool isRollback = false, int version = 0);
}