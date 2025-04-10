# 版本控制

Biwen.QuickApi 的 CMS 模块提供了强大的内容版本控制功能，支持内容的版本管理、比较和回滚。

## 基本概念

### 1. 版本创建

每次内容更新时自动创建新版本：

```csharp
public class ArticleService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(
        IContentRepository repository,
        ILogger<ArticleService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Article> UpdateArticleAsync(Guid id, UpdateArticleDto dto)
    {
        var content = await _repository.GetByIdAsync(id);
        if (content == null)
        {
            throw new ContentNotFoundException(id);
        }

        var article = content.ToContent<Article>();
        article.Title.Value = dto.Title;
        article.Content.Value = dto.Content;
        article.Author.Value = dto.Author;
        
        // 更新内容时会自动创建新版本
        await _repository.UpdateAsync(content);
        return article;
    }
}
```

### 2. 版本查询

查询内容的版本历史：

```csharp
public class ArticleVersionService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleVersionService> _logger;

    public ArticleVersionService(
        IContentRepository repository,
        ILogger<ArticleVersionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ContentVersion>> GetVersionsAsync(Guid contentId)
    {
        return await _repository.GetVersionsAsync(contentId);
    }

    public async Task<ContentVersion> GetVersionAsync(Guid contentId, int version)
    {
        return await _repository.GetVersionAsync(contentId, version);
    }
}
```

## 高级功能

### 1. 版本比较

比较不同版本之间的差异：

```csharp
public class VersionComparisonService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<VersionComparisonService> _logger;

    public VersionComparisonService(
        IContentRepository repository,
        ILogger<VersionComparisonService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<VersionDiff> CompareVersionsAsync(
        Guid contentId, 
        int version1, 
        int version2)
    {
        var v1 = await _repository.GetVersionAsync(contentId, version1);
        var v2 = await _repository.GetVersionAsync(contentId, version2);
        
        return await _repository.CompareVersionsAsync(v1, v2);
    }
}
```

### 2. 版本回滚

回滚到指定版本：

```csharp
public class VersionRollbackService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<VersionRollbackService> _logger;

    public VersionRollbackService(
        IContentRepository repository,
        ILogger<VersionRollbackService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Article> RollbackToVersionAsync(
        Guid contentId, 
        int version)
    {
        try
        {
            var content = await _repository.GetByIdAsync(contentId);
            if (content == null)
            {
                throw new ContentNotFoundException(contentId);
            }

            var versionContent = await _repository.GetVersionAsync(contentId, version);
            if (versionContent == null)
            {
                throw new VersionNotFoundException(contentId, version);
            }

            await _repository.RollbackToVersionAsync(contentId, version);
            return content.ToContent<Article>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回滚版本失败: {ContentId}, {Version}", 
                contentId, version);
            throw;
        }
    }
}
```

## 版本配置

### 1. 全局配置

在 `Program.cs` 中配置版本控制选项：

```csharp
builder.Services.AddBiwenContents(options =>
{
    options.EnableVersioning = true;          // 启用版本控制
    options.MaxVersions = 10;                 // 最大版本数
    options.EnableAuditLog = true;            // 启用审计日志
    options.AuditLogRetentionDays = 90;       // 审计日志保留天数
});
```

### 2. 内容类型配置

为特定内容类型配置版本控制：

```csharp
[Description("文章")]
public class Article : ContentBase<Article>
{
    [Display(Name = "标题")]
    [Required]
    public TextFieldType Title { get; set; } = null!;

    [Display(Name = "内容")]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType? Content { get; set; }

    public override string Content_Description => "文章内容";
}
```

## 最佳实践

1. **版本管理**
   - 合理设置版本数量限制
   - 定期清理旧版本
   - 记录版本变更原因

2. **性能优化**
   - 使用增量存储
   - 优化版本查询
   - 实现版本缓存

3. **安全性**
   - 控制版本访问权限
   - 记录版本操作日志
   - 实现版本审核机制

## 示例

### 1. 文章版本管理

```csharp
public class ArticleVersionManager
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleVersionManager> _logger;

    public ArticleVersionManager(
        IContentRepository repository,
        ILogger<ArticleVersionManager> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Article> CreateNewVersionAsync(
        Guid articleId, 
        UpdateArticleDto dto,
        string changeReason)
    {
        var content = await _repository.GetByIdAsync(articleId);
        if (content == null)
        {
            throw new ContentNotFoundException(articleId);
        }

        var article = content.ToContent<Article>();
        article.Title.Value = dto.Title;
        article.Content.Value = dto.Content;
        
        // 创建新版本并记录变更原因
        await _repository.CreateVersionAsync(content, new ContentVersion
        {
            ContentId = articleId,
            Version = content.Version + 1,
            CreatorId = "currentUser",
            CreatorName = "当前用户",
            CreatedAt = DateTime.UtcNow
        });
        
        return article;
    }

    public async Task<IEnumerable<ContentVersion>> GetVersionHistoryAsync(
        Guid articleId)
    {
        return await _repository.GetVersionsAsync(articleId);
    }
}
```

### 2. 版本审核系统

```csharp
public class VersionAuditSystem
{
    private readonly IContentRepository _repository;
    private readonly ILogger<VersionAuditSystem> _logger;

    public VersionAuditSystem(
        IContentRepository repository,
        ILogger<VersionAuditSystem> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> ApproveVersionAsync(
        Guid contentId, 
        int version,
        string approver)
    {
        var versionContent = await _repository.GetVersionAsync(contentId, version);
        if (versionContent == null)
        {
            throw new VersionNotFoundException(contentId, version);
        }
        
        // 记录审核操作
        await _repository.CreateAuditLogAsync(new ContentAuditLog
        {
            ContentId = contentId,
            Action = "VersionApproval",
            Details = $"版本 {version} 已审核通过",
            OperatorId = approver,
            OperatorName = "审核人",
            Timestamp = DateTime.UtcNow
        });
        
        return true;
    }
}
```

## 下一步

- [内容类型定义](ContentTypes.md)
- [字段类型](FieldTypes.md)
- [内容渲染](ContentRendering.md)
