// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.Contents.Services;

/// <summary>
/// 内容审计日志服务实现类
/// </summary>
public class ContentAuditLogService : IContentAuditLogService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContentAuditLogService(
        IServiceScopeFactory serviceScopeFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAuditAsync(Guid contentId, string action, string details, string? operatorId = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        var currentOperatorId = operatorId ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var currentOperatorName = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        var auditLog = new ContentAuditLog
        {
            Id = Guid.NewGuid(),
            ContentId = contentId,
            Action = action,
            Details = details,
            OperatorId = currentOperatorId,
            OperatorName = currentOperatorName,
            Timestamp = DateTime.Now
        };

        dbContext.ContentAuditLogs.Add(auditLog);
        await dbContext.Context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ContentAuditLog>> GetAuditLogsAsync(Guid contentId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        return await dbContext.ContentAuditLogs
            .Where(l => l.ContentId == contentId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<IPagedList<ContentAuditLog>> GetAuditLogsAsync(DateTime startTime, DateTime endTime, int pageIndex = 0, int pageSize = 10)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        var query = dbContext.ContentAuditLogs
            .Where(l => l.Timestamp >= startTime && l.Timestamp <= endTime)
            .OrderByDescending(l => l.Timestamp);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }
}
