// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 21:54:43 ContentRepository.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.Events;
using Biwen.QuickApi.Events;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.Contents;

public class ContentRepository(
    ContentSerializer serializer,
    IContentDbContext dbContext,
    IContentSchemaGenerator schemaGenerator) :
    IContentRepository
{
    public async Task<Guid> SaveContentAsync<T>(T content, string? title = null, string? slug = null) where T : IContent
    {
        // 获取内容类型的完全限定名
        var contentType = content.GetType().FullName;

        // 序列化内容字段值
        var jsonContent = serializer.SerializeContent(content);

        // 确定标题（从传入参数或从内容中获取）
        var contentTitle = title;
        if (string.IsNullOrEmpty(contentTitle))
        {
            // 尝试从内容中获取Title字段
            var titleProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Title", StringComparison.OrdinalIgnoreCase) &&
                                    typeof(IFieldType).IsAssignableFrom(p.PropertyType));

            if (titleProperty != null)
            {
                var titleField = titleProperty.GetValue(content) as IFieldType;
                contentTitle = titleField?.ConvertToString(null); // 从字段中获取值
            }

            // 如果仍然没有标题，使用类型名称
            if (string.IsNullOrEmpty(contentTitle))
            {
                contentTitle = contentType!.Split('.').Last();
            }
        }

        // 创建新的Content实体
        var entity = new Content
        {
            Id = Guid.NewGuid(),
            Slug = slug ?? contentTitle!.ToLower().Replace(" ", "-"),
            Title = contentTitle!,
            ContentType = contentType!,
            JsonContent = jsonContent,
            CreatedAt = DateTime.Now,
            Status = ContentStatus.Draft
        };

        dbContext.Contents.Add(entity);
        await dbContext.Context.SaveChangesAsync();

        // 发布内容创建事件
        await new ContentCreatedEvent(entity).PublishAsync();

        return entity.Id;
    }

    public async Task<T?> GetContentAsync<T>(Guid id) where T : IContent, new()
    {
        var entity = await dbContext.Context.Set<Content>().FindAsync(id);
        if (entity == null)
            return default;

        return serializer.DeserializeContent<T>(entity.JsonContent);
    }

    public async Task<IPagedList<T>> GetContentsByTypeAsync<T>(int pageIndex = 0, int len = 10) where T : IContent, new()
    {
        var contentType = typeof(T).FullName;

        var queryable = dbContext.Contents
            .Where(c => c.ContentType == contentType)
            .OrderByDescending(c => c.CreatedAt);

        return await queryable.Select(e => serializer.DeserializeContent<T>(e.JsonContent))
            .Where(c => c != null)
            .ToPagedListAsync(pageIndex, len);
    }

    /// <summary>
    /// 获取Content
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="slug"></param>
    /// <param name="pageIndex"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public async Task<IPagedList<Content>> GetDomainContentsByTypeAsync<T>(
        string? slug = null,
        int pageIndex = 0,
        int len = 10,
        int? status = null,
        string? title = null
        ) where T : IContent, new()
    {
        var contentType = typeof(T).FullName;
        var queryable = dbContext.Contents
            .Where(c => c.ContentType == contentType && (slug == null ? true : c.Slug == slug))
            .Where(c => status == null ? true : c.Status == (ContentStatus)status)
            .Where(c => title == null ? true : c.Title.Contains(title))
            .OrderByDescending(c => c.CreatedAt);

        return await queryable.ToPagedListAsync(pageIndex, len);
    }

    public async Task<T?> GetContentsByTypeAsync<T>(string slug) where T : IContent, new()
    {
        var contentType = typeof(T).FullName;
        var item = await dbContext.Contents
            .Where(c => c.ContentType == contentType && c.Slug == slug)
            .FirstOrDefaultAsync();
        if (item == null)
            return default;

        return serializer.DeserializeContent<T>(item.JsonContent);
    }

    public async Task UpdateContentAsync<T>(Guid id, T content) where T : IContent
    {
        var entity = await dbContext.Contents.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Content with ID {id} not found");

        // 保存旧版本的内容以用于事件发布
        var previousVersion = entity.JsonContent;

        // 更新内容
        entity.JsonContent = serializer.SerializeContent(content);
        entity.UpdatedAt = DateTime.Now;

        await dbContext.Context.SaveChangesAsync();

        // 发布内容更新事件
        await new ContentUpdatedEvent(entity, previousVersion).PublishAsync();
    }

    public async Task DeleteContentAsync(Guid id)
    {
        var entity = await dbContext.Context.Set<Content>().FindAsync(id);
        if (entity == null)
            return;

        dbContext.Context.Set<Content>().Remove(entity);
        await dbContext.Context.SaveChangesAsync();

        // 发布内容删除事件
        await new ContentDeletedEvent(id).PublishAsync();
    }

    /// <summary>
    /// 设置发布状态
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task SetContentStatusAsync(Guid id, ContentStatus status)
    {
        var entity = await dbContext.Context.Set<Content>().FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Content with ID {id} not found");

        // 记录旧状态
        var previousStatus = entity.Status;

        // 更新状态
        entity.Status = status;
        entity.UpdatedAt = DateTime.Now;

        if (status == ContentStatus.Published)
            entity.PublishedAt = DateTime.Now;

        await dbContext.Context.SaveChangesAsync();

        // 发布状态变更事件
        if (previousStatus != status)
        {
            await new ContentStatusChangedEvent(entity, previousStatus, status).PublishAsync();
        }
    }

    /// <summary>
    /// 获取原始内容实体
    /// </summary>
    /// <param name="id">内容ID</param>
    /// <returns>内容实体</returns>
    public async Task<Content> GetRawContentAsync(Guid id)
    {
        var entity = await dbContext.Contents.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"未找到ID为 {id} 的内容");

        return entity;
    }

    /// <summary>
    /// 更新原始内容实体
    /// </summary>
    /// <param name="content">内容实体</param>
    public async Task UpdateRawContentAsync(Content content, bool isRollback = false, int version = 0)
    {
        // 保存旧版本的内容以用于事件发布
        var previousVersion = content.JsonContent;

        // 只更新必要的字段
        content.UpdatedAt = DateTime.Now;

        // 标记实体为已修改
        dbContext.Context.Entry(content).State = EntityState.Modified;
        await dbContext.Context.SaveChangesAsync();

        // 发布内容更新事件
        await new ContentUpdatedEvent(content, previousVersion, isRollback, version).PublishAsync();
    }

    // 获取内容的Schema
    public string GetContentSchema<T>() where T : IContent
    {
        return schemaGenerator.GenerateSchemaJson<T>();
    }
}
