// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.Events;
using Biwen.QuickApi.Contents.Services;
using Biwen.QuickApi.Infrastructure.Locking;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Contents.EventHandlers;

/// <summary>
/// 内容版本控制事件处理器
/// </summary>
public class ContentVersionEventHandler(
    ILogger<ContentVersionEventHandler> logger,
    IContentVersionService contentVersionService,
    ILocalLock @lock,
    IHttpContextAccessor httpContextAccessor) :
    IEventSubscriber<ContentUpdatedEvent>
{

    public async Task HandleAsync(ContentUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取当前用户ID
            var creatorId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            //使用ILocalLock进行锁定,防止重复执行
            var (locker, _) = await @lock.TryAcquireLockAsync(
                $"{nameof(ContentVersionEventHandler)}.{@event.Content.Id}", TimeSpan.FromSeconds(20));
            using (locker)
            {
                // 查询当前最大版本号
                var currentMaxVersion = await contentVersionService.GetLatestVersionAsync(@event.Content.Id) is { } latestVersion
                    ? latestVersion.Version
                    : 0;

                // 创建新版本
                var newVersion = new ContentVersion
                {
                    Id = Guid.NewGuid(),
                    ContentId = @event.Content.Id,
                    Version = currentMaxVersion + 1,
                    Snapshot = @event.PreviousVersion,
                    CreatedAt = DateTime.Now,
                    CreatorId = creatorId
                };

                // 将新版本添加到数据库
                await contentVersionService.CreateVersionAsync(newVersion.ContentId, newVersion.Version, newVersion.Snapshot);
                logger.LogInformation("内容 {@ContentId} 的新版本 {Version} 已创建", @event.Content.Id, newVersion.Version);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "处理内容更新事件创建版本快照时出错");
        }
    }
}