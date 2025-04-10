# CMS 概述

Biwen.QuickApi 的 CMS 模块提供了一个灵活的内容管理系统，支持多种内容类型和自定义字段。

## 基本概念

### 1. 内容类型

内容类型是 CMS 的核心概念，它定义了内容的结构和行为：

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

    [Display(Name = "发布日期")]
    public DateTimeFieldType? PublishDate { get; set; }

    public override string Content_Description => "文章内容";
}
```

### 2. 字段类型

CMS 支持多种字段类型：

- 文本字段（`TextFieldType`）
- 富文本字段（`MarkdownFieldType`）
- 日期字段（`DateTimeFieldType`）
- 数字字段（`NumberFieldType`）
- 选择字段（`OptionsFieldType`）
- 媒体字段（`MediaFieldType`）

## 使用方式

### 1. 基本配置

在 `Program.cs` 中配置 CMS：

```csharp
builder.Services.AddBiwenContents(options =>
{
    options.EnableVersioning = true;
    options.EnableAuditLog = true;
    options.MaxPageSize = 100;
});
```

### 2. 内容管理

使用 `IContentRepository` 管理内容：

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

    public async Task<Article> CreateArticleAsync(CreateArticleDto dto)
    {
        try
        {
            var content = new Content
            {
                ContentType = typeof(Article).FullName,
                Title = dto.Title,
                Slug = dto.Slug,
                Status = ContentStatus.Draft
            };

            var article = content.ToContent<Article>();
            article.Title.Value = dto.Title;
            article.Content.Value = dto.Content;
            article.PublishDate.Value = DateTime.UtcNow;

            await _repository.CreateAsync(content);
            return article;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建文章失败");
            throw;
        }
    }
}
```

## 高级特性

### 1. 版本控制

CMS 支持内容版本控制：

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

    public async Task<Article> GetVersionAsync(Guid contentId, int version)
    {
        var versionContent = await _repository.GetVersionAsync(contentId, version);
        if (versionContent == null)
        {
            throw new VersionNotFoundException(contentId, version);
        }
        return versionContent.ToContent<Article>();
    }

    public async Task<IEnumerable<ContentVersion>> GetVersionsAsync(Guid contentId)
    {
        return await _repository.GetVersionsAsync(contentId);
    }
}
```

## 最佳实践

1. **内容设计**
   - 合理设计内容类型
   - 使用适当的字段类型
   - 考虑内容的扩展性

2. **性能优化**
   - 使用缓存
   - 优化查询
   - 批量处理内容

3. **安全考虑**
   - 实现访问控制
   - 验证内容
   - 保护敏感信息

## 示例

### 1. 文章管理示例

```csharp
[QuickApi("articles")]
public class ArticleApi : BaseQuickApi
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleApi> _logger;

    public ArticleApi(
        IContentRepository repository,
        ILogger<ArticleApi> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Article> Get(Guid id)
    {
        var content = await _repository.GetByIdAsync(id);
        if (content == null)
        {
            throw new ContentNotFoundException(id);
        }
        return content.ToContent<Article>();
    }

    [HttpPost]
    public async Task<Result> Create([FromBody] CreateArticleDto dto)
    {
        try
        {
            var content = new Content
            {
                ContentType = typeof(Article).FullName,
                Title = dto.Title,
                Slug = dto.Slug,
                Status = ContentStatus.Draft
            };

            var article = content.ToContent<Article>();
            article.Title.Value = dto.Title;
            article.Content.Value = dto.Content;
            article.PublishDate.Value = DateTime.UtcNow;

            await _repository.CreateAsync(content);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建文章失败");
            return Result.Fail("创建文章失败");
        }
    }
}
```

### 2. 内容查询示例

```csharp
public class ContentQueryService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ContentQueryService> _logger;

    public ContentQueryService(
        IContentRepository repository,
        ILogger<ContentQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
    {
        return await _repository
            .Query<Article>()
            .Where(x => x.PublishDate.Value <= DateTime.UtcNow)
            .OrderByDescending(x => x.PublishDate.Value)
            .ToListAsync();
    }
}
```

## 下一步

- [内容类型定义](ContentTypes.md)
- [字段类型](FieldTypes.md)
- [内容渲染](ContentRendering.md)
