// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.UnitOfWork.Pagenation;

namespace Biwen.QuickApi.Contents.Services;

/// <summary>
/// 内容审计日志服务接口
/// </summary>
public interface IContentAuditLogService
{
    /// <summary>
    /// 记录审计日志
    /// </summary>
    /// <param name="contentId">内容ID</param>
    /// <param name="action">操作类型</param>
    /// <param name="details">操作详情</param>
    /// <param name="operatorId">操作者ID</param>
    /// <returns></returns>
    Task LogAuditAsync(Guid contentId, string action, string details, string? operatorId = null);

    /// <summary>
    /// 获取指定内容的审计日志
    /// </summary>
    /// <param name="contentId">内容ID</param>
    /// <returns></returns>
    Task<IEnumerable<ContentAuditLog>> GetAuditLogsAsync(Guid contentId);

    /// <summary>
    /// 根据时间范围获取审计日志
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="pageIndex">页码，从0开始</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns></returns>
    Task<IPagedList<ContentAuditLog>> GetAuditLogsAsync(DateTime startTime, DateTime endTime, int pageIndex = 0, int pageSize = 10);
}