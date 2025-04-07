// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Contents.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Contents.Services;

/// <summary>
/// 内容版本服务实现类
/// </summary>
public class ContentVersionService : IContentVersionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ContentVersionService(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task CreateVersionAsync(Guid contentId, int version, string snapshot)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var currentUserName = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        var contentVersion = new ContentVersion
        {
            Id = Guid.NewGuid(),
            ContentId = contentId,
            Version = version,
            Snapshot = snapshot,
            CreatedAt = DateTime.Now,
            CreatorId = currentUserId,
            CreatorName = currentUserName
        };

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        dbContext.ContentVersions.Add(contentVersion);
        await dbContext.Context.SaveChangesAsync();
    }

    public async Task<ContentVersion?> GetVersionAsync(Guid contentId, Guid version)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        return await dbContext.ContentVersions
            .FirstOrDefaultAsync(v => v.Id == version && v.ContentId == contentId);
    }

    public async Task<IEnumerable<ContentVersion>> GetVersionsAsync(Guid contentId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        return await dbContext.ContentVersions
            .Where(v => v.ContentId == contentId)
            .OrderByDescending(v => v.Version)
            .ToListAsync();
    }

    public async Task<ContentVersion?> GetLatestVersionAsync(Guid contentId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IContentDbContext>();

        return await dbContext.ContentVersions
            .Where(v => v.ContentId == contentId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync();
    }
}