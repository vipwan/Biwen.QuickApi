// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.Services;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Contents.Apis;


[AutoDto<ContentVersion>]
[AutoDtoComplex(2)]//现在支持复杂类型DTO生成
public partial record ContentVersionDto;

/// <summary>
/// 获取内容版本列表的API
/// </summary>
[QuickApi("/versions/{id:guid}", Group = Constants.GroupName)]
[OpenApiMetadata("获取内容版本列表", "获取指定内容的所有版本")]
public class GetContentVersionsApi(IContentVersionService versionService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, IEnumerable<ContentVersionDto>>
{
    public override async ValueTask<IEnumerable<ContentVersionDto>> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的ID");
        }

        var models = await versionService.GetVersionsAsync(contentId);
        //对象转换
        return models.AsQueryable().ProjectToContentVersionDto();
    }
}

/// <summary>
/// 获取指定版本内容的API
/// </summary>
[QuickApi("/versions/{id:guid}/{version:guid}", Group = Constants.GroupName)]
[OpenApiMetadata("获取指定版本内容", "获取指定内容的指定版本")]
public class GetContentVersionApi(IContentVersionService versionService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, ContentVersionDto?>
{
    public override async ValueTask<ContentVersionDto?> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        var version = httpContextAccessor.HttpContext!.Request.RouteValues["version"]?.ToString();

        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的ID");
        }

        if (string.IsNullOrEmpty(version) || !Guid.TryParse(version, out var versionNumber))
        {
            throw new ArgumentException("无效的版本号");
        }

        var model = await versionService.GetVersionAsync(contentId, versionNumber);

        if (model == null)
            throw new ArgumentException("无效的版本号");

        //对象转换
        return model.MapperToContentVersionDto();
    }
}


//contents/versions/f67d329e-f54f-4e3b-be0c-2e0e3652d9f3/rollback/16b4a343-24ca-4452-9f23-43da7711ec1a
//实现回滚接口

/// <summary>
/// 内容版本回滚API
/// </summary>
[QuickApi("/versions/{id:guid}/rollback/{version:guid}", Verbs = Verb.POST, Group = Constants.GroupName)]
[OpenApiMetadata("回滚内容版本", "将内容回滚到指定版本")]
public class RollbackContentVersionApi(
    IContentVersionService versionService,
    IContentRepository contentRepository,
    IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, bool>
{
    public override async ValueTask<bool> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        var version = httpContextAccessor.HttpContext!.Request.RouteValues["version"]?.ToString();

        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的内容ID");
        }

        if (string.IsNullOrEmpty(version) || !Guid.TryParse(version, out var versionId))
        {
            throw new ArgumentException("无效的版本ID");
        }

        // 获取要回滚到的版本
        var versionToRollback = await versionService.GetVersionAsync(contentId, versionId);
        if (versionToRollback == null)
        {
            throw new ArgumentException("找不到指定的内容版本");
        }

        // 获取当前内容
        var content = await contentRepository.GetRawContentAsync(contentId);

        // 保存版本之前的原始内容
        string originalContent = content.JsonContent;

        // 更新内容为版本的内容
        content.JsonContent = versionToRollback.Snapshot;
        content.UpdatedAt = DateTime.Now;

        // 更新内容
        await contentRepository.UpdateRawContentAsync(content, true, versionToRollback.Version);

        return true;
    }
}

