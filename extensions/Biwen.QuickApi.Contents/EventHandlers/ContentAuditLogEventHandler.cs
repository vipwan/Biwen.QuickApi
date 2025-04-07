// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Contents.Events;
using Biwen.QuickApi.Contents.Services;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Contents.EventHandlers;

/// <summary>
/// 内容审计日志事件处理器
/// </summary>
public class ContentAuditLogEventHandler(
    ILogger<ContentAuditLogEventHandler> logger,
    IContentAuditLogService contentAuditLogService,
    IHttpContextAccessor httpContextAccessor) :
    IEventSubscriber<ContentCreatedEvent>,
    IEventSubscriber<ContentUpdatedEvent>,
    IEventSubscriber<ContentDeletedEvent>,
    IEventSubscriber<ContentStatusChangedEvent>
{
    public async Task HandleAsync(ContentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await contentAuditLogService.LogAuditAsync(
                @event.Content.Id,
                "创建内容",
                $"标题: {@event.Content.Title}, 类型: {@event.Content.ContentType}",
                operatorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理内容创建事件记录审计日志时出错");
        }
    }

    public async Task HandleAsync(ContentUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (@event.IsRollback)
            {
                await contentAuditLogService.LogAuditAsync(
                    @event.Content.Id,
                    "回滚内容",
                    $"标题: {@event.Content.Title}, 类型: {@event.Content.ContentType}, 版本: {@event.RollbackVersion}",
                    operatorId);
                return;
            }
            else
            {
                await contentAuditLogService.LogAuditAsync(
                    @event.Content.Id,
                    "更新内容",
                    $"标题: {@event.Content.Title}, 类型: {@event.Content.ContentType}",
                    operatorId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理内容更新事件记录审计日志时出错");
        }
    }

    public async Task HandleAsync(ContentDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await contentAuditLogService.LogAuditAsync(
                @event.ContentId,
                "删除内容",
                $"删除内容ID: {@event.ContentId}",
                operatorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理内容删除事件记录审计日志时出错");
        }
    }

    public async Task HandleAsync(ContentStatusChangedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await contentAuditLogService.LogAuditAsync(
                @event.Content.Id,
                "状态变更",
                $"内容状态从 {@event.PreviousStatus} 变更为 {@event.NewStatus}",
                operatorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理内容状态变更事件记录审计日志时出错");
        }
    }
}
