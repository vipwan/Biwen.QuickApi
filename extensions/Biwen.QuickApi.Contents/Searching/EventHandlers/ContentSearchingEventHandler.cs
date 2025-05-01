// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-05-02 04:31:30 ContentSearchingEventHandler.cs

using Biwen.QuickApi.Contents.Events;

namespace Biwen.QuickApi.Contents.Searching.EventHandlers;

/// <summary>
/// 内容搜索事件处理器
/// 通过事件处理机制在内容变更时自动更新ElasticSearch索引
/// 使用IServiceScopeFactory动态解析IContentSearchService，
/// 这样当系统未启用ElasticSearch时，也不会抛出依赖注入错误
/// </summary>
public class ContentSearchingEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ContentSearchingEventHandler> logger) :
    IEventSubscriber<ContentCreatedEvent>,
    IEventSubscriber<ContentUpdatedEvent>,
    IEventSubscriber<ContentDeletedEvent>,
    IEventSubscriber<ContentStatusChangedEvent>
{
    /// <summary>
    /// 处理内容创建事件
    /// </summary>
    public async Task HandleAsync(ContentCreatedEvent @event, CancellationToken ct)
    {
        // 获取内容实体
        var content = @event.Content;

        // 尝试更新索引
        await TryUpdateSearchIndexAsync(async searchService =>
        {
            await searchService.AddOrUpdateDocumentAsync(content);
            logger.LogInformation("已将新创建的内容添加到搜索索引，内容ID: {ContentId}", content.Id);
        });
    }

    /// <summary>
    /// 处理内容更新事件
    /// </summary>
    public async Task HandleAsync(ContentUpdatedEvent @event, CancellationToken ct)
    {
        // 获取内容实体
        var content = @event.Content;

        // 尝试更新索引
        await TryUpdateSearchIndexAsync(async searchService =>
        {
            await searchService.AddOrUpdateDocumentAsync(content);
            logger.LogInformation("已更新搜索索引中的内容，内容ID: {ContentId}", content.Id);
        });
    }

    /// <summary>
    /// 处理内容删除事件
    /// </summary>
    public async Task HandleAsync(ContentDeletedEvent @event, CancellationToken ct)
    {
        // 获取内容ID
        var contentId = @event.ContentId;

        // 尝试更新索引
        await TryUpdateSearchIndexAsync(async searchService =>
        {
            await searchService.DeleteDocumentAsync(contentId);
            logger.LogInformation("已从搜索索引中删除内容，内容ID: {ContentId}", contentId);
        });
    }

    /// <summary>
    /// 处理内容状态变更事件
    /// </summary>
    public async Task HandleAsync(ContentStatusChangedEvent @event, CancellationToken ct)
    {
        // 获取内容实体
        var content = @event.Content;

        // 尝试更新索引
        await TryUpdateSearchIndexAsync(async searchService =>
        {
            // 如果内容被存档，从索引中删除
            if (content.Status == Domain.ContentStatus.Archived)
            {
                await searchService.DeleteDocumentAsync(content.Id);
                logger.LogInformation("已从搜索索引中删除已归档的内容，内容ID: {ContentId}", content.Id);
            }
            // 否则更新索引中的内容
            else
            {
                await searchService.AddOrUpdateDocumentAsync(content);
                logger.LogInformation("已更新搜索索引中的内容状态，内容ID: {ContentId}, 新状态: {Status}",
                    content.Id, content.Status);
            }
        });
    }

    /// <summary>
    /// 尝试执行搜索索引操作，如果搜索服务不可用则不执行任何操作
    /// </summary>
    /// <param name="action">要执行的索引操作</param>
    private async Task TryUpdateSearchIndexAsync(Func<IContentSearchService, Task> action)
    {
        try
        {
            // 创建作用域以解析服务
            using var scope = serviceScopeFactory.CreateScope();

            // 尝试获取搜索服务
            var searchService = scope.ServiceProvider.GetService<IContentSearchService>();

            // 如果搜索服务可用，执行操作
            if (searchService != null)
            {
                // 检查ES服务是否正常运行
                if (await searchService.HealthCheckAsync())
                {
                    // 执行操作
                    await action(searchService);
                }
                else
                {
                    logger.LogWarning("Elasticsearch服务不可用，无法更新搜索索引");
                }
            }
            else
            {
                logger.LogDebug("系统未启用Elasticsearch搜索服务，跳过索引更新");
            }
        }
        catch (Exception ex)
        {
            // 记录但不抛出异常，避免影响主业务流程
            logger.LogError(ex, "更新搜索索引时发生错误");
        }
    }
}
