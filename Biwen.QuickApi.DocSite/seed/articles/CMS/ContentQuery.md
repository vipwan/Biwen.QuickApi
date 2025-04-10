# 内容查询

Biwen.QuickApi 的 CMS 模块提供了强大的内容查询功能，支持复杂的查询条件和灵活的过滤选项。

## 基本查询

### 1. 简单查询

使用 `IContentRepository` 进行基本查询：

```csharp
public class ArticleQueryService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleQueryService> _logger;

    public ArticleQueryService(
        IContentRepository repository,
        ILogger<ArticleQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
    {
        return await _repository
            .Query<Article>()
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.PublishDate)
            .ToListAsync();
    }
}
```

### 2. 分页查询

实现分页查询：

```csharp
public class ArticlePaginationService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticlePaginationService> _logger;

    public ArticlePaginationService(
        IContentRepository repository,
        ILogger<ArticlePaginationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<Article>> GetArticlesAsync(
        int pageIndex, 
        int pageSize)
    {
        var query = _repository.Query<Article>();
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.PublishDate)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Article>
        {
            Items = items,
            Total = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}
```

## 高级查询

### 1. 条件查询

使用复杂条件进行查询：

```csharp
public class ArticleSearchService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleSearchService> _logger;

    public ArticleSearchService(
        IContentRepository repository,
        ILogger<ArticleSearchService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> SearchArticlesAsync(
        ArticleSearchCriteria criteria)
    {
        var query = _repository.Query<Article>();

        if (!string.IsNullOrEmpty(criteria.Keyword))
        {
            query = query.Where(x => 
                x.Title.Value.Contains(criteria.Keyword) || 
                x.Content.Value.Contains(criteria.Keyword));
        }

        if (criteria.Category != null)
        {
            query = query.Where(x => x.Category.Value == criteria.Category);
        }

        if (criteria.StartDate.HasValue)
        {
            query = query.Where(x => x.PublishDate.Value >= criteria.StartDate.Value);
        }

        if (criteria.EndDate.HasValue)
        {
            query = query.Where(x => x.PublishDate.Value <= criteria.EndDate.Value);
        }

        return await query
            .OrderByDescending(x => x.PublishDate.Value)
            .ToListAsync();
    }
}
```

### 2. 关联查询

查询关联内容：

```csharp
public class RelatedContentService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<RelatedContentService> _logger;

    public RelatedContentService(
        IContentRepository repository,
        ILogger<RelatedContentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> GetRelatedArticlesAsync(
        Guid articleId)
    {
        var content = await _repository.GetByIdAsync(articleId);
        if (content == null)
        {
            throw new ContentNotFoundException(articleId);
        }

        var article = content.ToContent<Article>();
        return await _repository
            .Query<Article>()
            .Where(x => 
                x.Category.Value == article.Category.Value && 
                x.Id != articleId)
            .Take(5)
            .ToListAsync();
    }
}
```

## 查询配置

### 1. 全局配置

在 `Program.cs` 中配置查询选项：

```csharp
builder.Services.AddBiwenContents(options =>
{
    options.EnableQueryCache = true;
    options.QueryCacheDuration = TimeSpan.FromMinutes(30);
    options.MaxPageSize = 100;
    options.EnableQueryLogging = true;
});
```

### 2. 查询优化

使用查询优化器：

```csharp
public class QueryOptimizer : IQueryOptimizer
{
    private readonly ILogger<QueryOptimizer> _logger;

    public QueryOptimizer(ILogger<QueryOptimizer> logger)
    {
        _logger = logger;
    }

    public IQueryable<T> Optimize<T>(IQueryable<T> query)
    {
        // 实现查询优化逻辑
        return query;
    }
}
```

## 最佳实践

1. **性能优化**
   - 使用索引优化查询
   - 实现查询缓存
   - 优化复杂查询
   - 使用异步操作

2. **可维护性**
   - 使用查询构建器
   - 实现查询过滤器
   - 提供查询日志
   - 遵循 SOLID 原则

3. **安全性**
   - 验证查询参数
   - 控制查询范围
   - 实现访问控制
   - 防止 SQL 注入

## 示例

### 1. 文章查询服务

```csharp
public class ArticleQueryService
{
    private readonly IContentRepository _repository;
    private readonly ILogger<ArticleQueryService> _logger;

    public ArticleQueryService(
        IContentRepository repository,
        ILogger<ArticleQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> GetArticlesByCategoryAsync(
        string category, 
        bool includeDrafts = false)
    {
        try
        {
            var query = _repository.Query<Article>()
                .Where(x => x.Category.Value == category);

            if (!includeDrafts)
            {
                query = query.Where(x => x.IsPublished);
            }

            return await query
                .OrderByDescending(x => x.PublishDate.Value)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询文章失败: {Category}", category);
            throw;
        }
    }

    public async Task<IEnumerable<Article>> GetRecentArticlesAsync(int count)
    {
        return await _repository
            .Query<Article>()
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.PublishDate.Value)
            .Take(count)
            .ToListAsync();
    }
}
```

### 2. 高级搜索服务

```csharp
public class AdvancedSearchService
{
    private readonly IContentRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdvancedSearchService> _logger;

    public AdvancedSearchService(
        IContentRepository repository,
        IMemoryCache cache,
        ILogger<AdvancedSearchService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SearchResult<Article>> SearchAsync(
        SearchRequest request)
    {
        var cacheKey = $"search_{request.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out SearchResult<Article>? cachedResult))
        {
            return cachedResult!;
        }

        var query = _repository.Query<Article>();

        // 应用搜索条件
        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(x => 
                x.Title.Value.Contains(request.Keyword) || 
                x.Content.Value.Contains(request.Keyword));
        }

        // 执行查询
        var result = new SearchResult<Article>
        {
            Items = await query.ToListAsync(),
            Total = await query.CountAsync()
        };

        // 缓存结果
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }
}
```

## 下一步

- [内容类型定义](ContentTypes.md)
- [字段类型](FieldTypes.md)
- [内容渲染](ContentRendering.md) 