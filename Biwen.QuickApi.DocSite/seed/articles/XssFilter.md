# XSS 过滤

Biwen.QuickApi 提供了 XSS 过滤功能，用于防止跨站脚本攻击。

## 基本概念

### 1. XSS 过滤配置

在 `Program.cs` 中配置 XSS 过滤：

```csharp
builder.Services.AddXssFilter(options =>
{
    options.Enabled = true;
    options.AllowHtml = false;
    options.AllowScript = false;
});
```

### 2. 使用方式

在 API 中使用 `[XssFilter]` 特性：

```csharp
[QuickApi("content")]
public class ContentApi : BaseQuickApi
{
    [XssFilter]
    public async Task<Result> Create([FromBody] CreateContentDto dto)
    {
        // dto 中的内容会被自动过滤
        await _contentService.CreateAsync(dto);
        return Result.Success();
    }
}
```

## 使用方式

### 1. 基本使用

```csharp
public class UserApi : BaseQuickApi
{
    [XssFilter]
    public async Task<Result> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        // dto 中的内容会被自动过滤
        await _userService.UpdateProfileAsync(dto);
        return Result.Success();
    }
}
```

### 2. 配置选项

可以在 `appsettings.json` 中配置 XSS 过滤选项：

```json
{
  "XssFilter": {
    "Enabled": true,
    "AllowHtml": false,
    "AllowScript": false,
    "AllowedTags": ["p", "br", "strong"],
    "AllowedAttributes": ["class", "style"]
  }
}
```

## 高级特性

### 1. 自定义过滤规则

可以实现自定义的 XSS 过滤规则：

```csharp
public class CustomXssFilter : IXssFilter
{
    public string Filter(string input)
    {
        // 实现自定义过滤逻辑
        return Sanitize(input);
    }
}

// 注册自定义过滤器
builder.Services.AddSingleton<IXssFilter, CustomXssFilter>();
```

### 2. 白名单配置

可以配置允许的 HTML 标签和属性：

```csharp
builder.Services.AddXssFilter(options =>
{
    options.AllowedTags = new[] { "p", "br", "strong" };
    options.AllowedAttributes = new[] { "class", "style" };
});
```

## 最佳实践

1. **安全考虑**
   - 始终启用 XSS 过滤
   - 谨慎配置允许的 HTML 标签
   - 定期更新过滤规则

2. **性能优化**
   - 避免过度过滤
   - 使用缓存优化性能
   - 监控过滤性能

3. **错误处理**
   - 记录过滤失败的情况
   - 提供适当的错误信息
   - 实现回退机制

## 示例

### 1. 内容管理示例

```csharp
[QuickApi("articles")]
public class ArticleApi : BaseQuickApi
{
    private readonly IArticleService _articleService;
    private readonly ILogger<ArticleApi> _logger;

    public ArticleApi(IArticleService articleService, ILogger<ArticleApi> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    [XssFilter]
    public async Task<Result> Create([FromBody] CreateArticleDto dto)
    {
        try
        {
            _logger.LogInformation("开始创建文章");
            await _articleService.CreateAsync(dto);
            _logger.LogInformation("文章创建成功");
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

### 2. 评论系统示例

```csharp
[QuickApi("comments")]
public class CommentApi : BaseQuickApi
{
    [XssFilter(AllowHtml = true, AllowedTags = new[] { "p", "br" })]
    public async Task<Result> AddComment([FromBody] AddCommentDto dto)
    {
        // 允许基本的 HTML 标签
        await _commentService.AddAsync(dto);
        return Result.Success();
    }
}
```

## 下一步

- [请求绑定](ReqBinder.md)
- [验证](Validation.md)
- [审计](Auditing.md) 