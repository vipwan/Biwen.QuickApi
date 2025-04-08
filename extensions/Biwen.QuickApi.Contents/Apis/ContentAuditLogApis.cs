// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.Services;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Contents.Apis;

/// <summary>
/// 获取内容审计日志的API
/// </summary>
[QuickApi("/{id:guid}/auditlogs", Group = Constants.GroupName)]
[OpenApiMetadata("获取内容审计日志", "获取指定内容的所有审计日志")]
public class GetContentAuditLogsApi(
    IContentAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, IEnumerable<ContentAuditLogDto>>
{
    public override async ValueTask<IEnumerable<ContentAuditLogDto>> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的ID");
        }

        var logs = await auditLogService.GetAuditLogsAsync(contentId);

        // 转换为DTO，包含用户名
        var dtoItems = new List<ContentAuditLogDto>();
        foreach (var log in logs)
        {
            var dto = log.MapperToContentAuditLogDto();
            dtoItems.Add(dto);
        }
        return dtoItems;
    }
}

/// <summary>
/// 时间范围内的审计日志查询参数
/// </summary>
public class AuditLogQueryRequest : BaseRequest<AuditLogQueryRequest>
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; } = DateTime.Now.Date;

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 页码，从1开始
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int? PageSize { get; set; }
}


/// <summary>
/// 审计日志DTO
/// </summary>

[AutoDto<ContentAuditLog>]
[AutoDtoComplex(2)]//现在支持复杂类型DTO生成
public partial record ContentAuditLogDto;

/// <summary>
/// 时间范围内的审计日志查询API
/// </summary>
[QuickApi("/auditlogs", Group = Constants.GroupName)]
[OpenApiMetadata("查询审计日志", "按时间范围查询审计日志")]
public class QueryAuditLogsApi(IContentAuditLogService auditLogService) : BaseQuickApi<AuditLogQueryRequest, IPagedList<ContentAuditLogDto>>
{
    public override async ValueTask<IPagedList<ContentAuditLogDto>> ExecuteAsync(AuditLogQueryRequest request, CancellationToken cancellationToken = default)
    {
        var pageIndex = (request.PageNumber ?? 1) - 1;
        var pageSize = request.PageSize ?? 10;

        // 获取分页的审计日志
        var logs = await auditLogService.GetAuditLogsAsync(
            request.StartTime ?? DateTime.Now.Date,
            request.EndTime ?? DateTime.Now,
            pageIndex,
            pageSize
        );

        // 转换为DTO，包含用户名
        var dtoItems = new List<ContentAuditLogDto>();
        foreach (var log in logs.Items)
        {
            var dto = log.MapperToContentAuditLogDto();
            dtoItems.Add(dto);
        }

        // 创建分页DTO
        return new PagedList<ContentAuditLogDto>(
            dtoItems,
            logs.PageIndex,
            logs.PageSize,
            logs.IndexFrom,
            logs.TotalCount
        );
    }
}