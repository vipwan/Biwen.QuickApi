// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.Contents.Apis;

/// <summary>
/// 查询请求参数
/// </summary>
public class PageRequest : BaseRequest<PageRequest>
{
    [FromQuery]
    [Description("页码:从1开始,默认:1")]
    public int? PageNumber { get; set; } = 1;

    [FromQuery]
    [Description("分页大小,默认:10")]
    public int? PageSize { get; set; } = 10;

    [FromQuery]
    [Description("文档类型")]
    public string? ContentType { get; set; }

    [FromQuery]
    [Description("Slug,如果不为空,查询为精准查询!")]
    public string? Slug { get; set; }

    [FromQuery]
    [Description("标题")]
    public string? Title { get; set; }

    [FromQuery]
    [Range(0, 2)]
    [Description("状态,0:草稿,1:已发布,2:归档")]
    public int? Status { get; set; }

}

[QuickApi("/infopages", Group = Constants.GroupName)]
[OpenApiMetadata("内容API", "文档查询")]
public class ContentsApi(
    IContentRepository repository,
    ContentSerializer contentSerializer
) : BaseQuickApi<PageRequest, IPagedList<Content>>
{
    public override async ValueTask<IPagedList<Content>> ExecuteAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var pageIndex = (request.PageNumber ?? 1) - 1;
        var pageSize = request.PageSize ?? 10;

        if (string.IsNullOrEmpty(request.ContentType))
        {
            return new PagedList<Content>([], 0, 1, 0, 10);
            //throw new ArgumentException("ContentType不能为空");
        }

        var types = contentSerializer.GetAllContentTypes();
        var contentType = types.FirstOrDefault(t => t.Content_ContentType == request.ContentType);
        if (contentType == null)
        {
            throw new ArgumentException($"不支持的文档类型: {request.ContentType}");
        }

        var method = repository!.GetType()
            .GetMethod(nameof(IContentRepository.GetDomainContentsByTypeAsync))
            ?.MakeGenericMethod(contentType.GetType());

        return await (Task<IPagedList<Content>>)method!.Invoke(
            repository,
            [request.Slug, pageIndex, pageSize, request.Status, request.Title]
        )!;
    }
}

[FromBody]
public class CreateContentRequest : BaseRequest<CreateContentRequest>
{
    [Required]
    [Description("标题")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Description("Slug")]
    public string? Slug { get; set; } = string.Empty;

    [Required]
    [Description("文档类型")]
    public string ContentType { get; set; } = null!;

    [Required]
    [Description("序列化的文档内容,为JSON数组,空:[]")]
    public string JsonContent { get; set; } = string.Empty;
}

[QuickApi("/create", Group = Constants.GroupName, Verbs = Verb.POST)]
[OpenApiMetadata("创建内容", "创建内容")]
public class CreateContentApi(
    IContentRepository repository,
    ContentSerializer contentSerializer
) : BaseQuickApi<CreateContentRequest, Guid>
{
    public override async ValueTask<Guid> ExecuteAsync(CreateContentRequest request, CancellationToken cancellationToken = default)
    {
        var types = contentSerializer.GetAllContentTypes();

        //同时满足完全限定名和 .最后一个名称
        var contentType = types.FirstOrDefault(t =>
        t.Content_ContentType == request.ContentType ||
        t.Content_ContentType.Split('.').Last() == request.ContentType);
        if (contentType == null)
        {
            throw new ArgumentException($"不支持的文档类型: {request.ContentType}");
        }

        var deserializeMethod = contentSerializer.GetType()
            .GetMethod(nameof(contentSerializer.DeserializeContent))
            ?.MakeGenericMethod(contentType.GetType());

        var content = deserializeMethod!.Invoke(contentSerializer, [request.JsonContent]);

        //使用反射获取SaveContentAsync的泛型方法并调用:
        var saveMethod = repository!.GetType()
            .GetMethod(nameof(IContentRepository.SaveContentAsync))
            ?.MakeGenericMethod(contentType.GetType());

        return await (Task<Guid>)saveMethod!.Invoke(repository, [content!, request.Title, request.Slug])!;
    }
}

[FromBody]
public class UpdateContentRequest : BaseRequest<UpdateContentRequest>
{
    [Required]
    [Description("标题")]
    public string Title { get; set; } = string.Empty;

    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug只能包含小写字母、数字和连字符")]
    public string? Slug { get; set; } = string.Empty;

    /// <summary>
    /// 状态只能是0(草稿)或1(已发布),2归档
    /// </summary>
    [Required]
    [Range(0, 2, ErrorMessage = "状态值无效，只能是0(草稿)或1(已发布),2归档")]
    public int Status { get; set; } = 0;

    [Required]
    [Description("文档类型,必填")]
    public string ContentType { get; set; } = null!;

    [Required]
    [Description("序列化的文档内容,为JSON数组,空:[]")]
    public string JsonContent { get; set; } = string.Empty;
}

[QuickApi("/{id:guid}", Group = Constants.GroupName, Verbs = Verb.PUT)]
[OpenApiMetadata("更新内容", "更新内容")]
public class UpdateContentApi(
    IContentRepository repository,
    ContentSerializer contentSerializer,
    IHttpContextAccessor httpContextAccessor
) : BaseQuickApi<UpdateContentRequest, bool>
{
    public override async ValueTask<bool> ExecuteAsync(UpdateContentRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        if (!Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的ID");
        }

        var types = contentSerializer.GetAllContentTypes();
        var contentType = types.FirstOrDefault(t =>
        t.Content_ContentType == request.ContentType ||
        t.Content_ContentType.Split('.').Last() == request.ContentType);
        if (contentType == null)
        {
            throw new ArgumentException($"不支持的文档类型: {request.ContentType}");
        }

        // 反序列化新内容
        var deserializeMethod = typeof(ContentSerializer)
            .GetMethod(nameof(ContentSerializer.DeserializeContent))
            ?.MakeGenericMethod(contentType.GetType());

        var updatedContent = deserializeMethod!.Invoke(contentSerializer, [request.JsonContent])!;

        //反射泛型方法:
        var updateMethod = repository!.GetType()
            .GetMethod(nameof(IContentRepository.UpdateContentAsync))
            ?.MakeGenericMethod(contentType.GetType());

        // 更新内容
        await (Task)updateMethod?.Invoke(repository, [contentId, updatedContent])!;


        //还需要更新标题和Slug
        var content = await repository.GetRawContentAsync(contentId);
        content.Title = request.Title;
        content.Slug = request.Slug!;
        await repository.UpdateRawContentAsync(content);

        // 更新状态
        var status = request.Status switch
        {
            0 => ContentStatus.Draft,
            1 => ContentStatus.Published,
            2 => ContentStatus.Archived,
            _ => throw new ArgumentException("无效的状态值")
        };

        await repository.SetContentStatusAsync(contentId, status);

        return true;
    }
}

[QuickApi("/{id:guid}", Group = Constants.GroupName, Verbs = Verb.DELETE)]
[OpenApiMetadata("删除内容", "删除指定ID的内容")]
public class DeleteContentApi(
    IContentRepository repository,
    IHttpContextAccessor httpContextAccessor
) : BaseQuickApi<EmptyRequest, bool>
{
    public override async ValueTask<bool> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        // 获取请求的ID
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var Id))
        {
            throw new ArgumentException("无效的ID");
        }

        try
        {
            await repository!.DeleteContentAsync(Id);
            return true;
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"未找到ID为{Id}的内容");
        }
        catch (Exception)
        {
            throw;
        }
    }
}

/// <summary>
/// 文档类型的数据传输对象
/// </summary>
[Description("文档类型的数据传输对象")]
public record ContentTypeDto([Description("文档类型")] string? ContentType, [Description("文档描述")] string? Description, [Description("排序")] int Order);

//获取所有的文档类型:
//api/contents/alltypes
[QuickApi("/alltypes", Group = Constants.GroupName)]
[OpenApiMetadata("获取所有文档类型", "获取所有文档类型")]
public class GetAllContentTypesApi(ContentSerializer contentSerializer) :
    BaseQuickApi<EmptyRequest, IEnumerable<ContentTypeDto>>
{
    public override async ValueTask<IEnumerable<ContentTypeDto>> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        // 获取所有的文档类型
        var types = contentSerializer.GetAllContentTypes();

        // 转换为DTO
        List<ContentTypeDto> retn = [];
        foreach (var item in types)
        {
            retn.Add(new ContentTypeDto(item.Content_ContentType, item.Content_Description, item.Content_Order));
        }

        return retn;
    }
}

//根据文档类型获取Schema:
//api/contents/schema/InfoPage

[QuickApi("/schema/{type}", Group = Constants.GroupName)]
[OpenApiMetadata("获取内容Schema", "获取指定类型的内容Schema")]
public class GetContentSchemaApi(
    IContentSchemaGenerator schemaGenerator,
    IHttpContextAccessor httpContextAccessor,
    ContentSerializer contentSerializer) :
    BaseQuickApi<EmptyRequest, string>
{
    public override async ValueTask<string> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // 获取请求的类型
        var type = httpContextAccessor.HttpContext!.Request.RouteValues["type"]?.ToString();
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("无效的类型");
        }

        var types = contentSerializer.GetAllContentTypes();
        var contentType = types.FirstOrDefault(t =>
        t.Content_ContentType == type ||
        t.Content_ContentType.Split('.').Last() == type);
        if (contentType == null)
        {
            throw new ArgumentException($"不支持的文档类型: {type}");
        }

        var schema = schemaGenerator.GenerateSchema(contentType.GetType());
        return schema.ToString();
    }
}