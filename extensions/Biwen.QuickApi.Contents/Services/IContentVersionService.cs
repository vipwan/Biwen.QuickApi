// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 21:22:15 IContentVersionService.cs

using Biwen.QuickApi.Contents.Domain;

namespace Biwen.QuickApi.Contents.Services;

/// <summary>
/// 内容版本服务接口
/// </summary>
public interface IContentVersionService
{
    /// <summary>
    /// 创建新版本
    /// </summary>
    /// <param name="contentId">内容ID</param>
    /// <param name="version">版本号</param>
    /// <param name="snapshot">内容快照</param>
    /// <returns></returns>
    Task CreateVersionAsync(Guid contentId, int version, string snapshot);

    /// <summary>
    /// 获取指定内容的所有版本
    /// </summary>
    /// <param name="contentId">内容ID</param>
    /// <returns></returns>
    Task<IEnumerable<ContentVersion>> GetVersionsAsync(Guid contentId);

    /// <summary>
    /// 获取指定内容的指定版本
    /// </summary>
    /// <param name="contentId">内容ID</param>
    /// <param name="version">版本Id</param>
    /// <returns></returns>
    Task<ContentVersion?> GetVersionAsync(Guid contentId, Guid version);


    /// <summary>
    /// 获取最新版本
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    Task<ContentVersion?> GetLatestVersionAsync(Guid contentId);

}