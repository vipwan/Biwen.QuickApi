# 内容渲染

Biwen.QuickApi.Contents 提供了灵活的内容渲染系统，支持通过 Razor 视图模板自定义内容的展示方式。本文档将详细介绍内容渲染的相关功能和使用方法。

## 渲染系统概述

### 渲染服务

核心渲染功能由 `IDocumentRenderService` 提供：

```csharp
public interface IDocumentRenderService
{
    Task<string> RenderAsync<T>(T content, string? viewName = null) where T : IContent;
    Task<string> RenderDocumentBySlugAsync(string slug);
    Task<string> RenderDocumentByIdAsync(Guid id);
}
```

### 视图模型

视图接收 `ContentViewModel<T>` 类型的模型：

```csharp
public class ContentViewModel<T> where T : IContent
{
    // 内容实例
    public T Content { get; set; } = default!;
    
    // 内容定义数据（元数据）
    public Content ContentDefine { get; set; } = default!;
}
```

## 基本用法

### 1. 视图模板

创建一个博客文章的视图模板 `Views/Contents/BlogPost.cshtml`：

```cshtml
@model Biwen.QuickApi.Contents.Rendering.ContentViewModel<BlogPost>
@{
    ViewData["Title"] = Model.Content.Title.Value;
    Layout = "_Layout";
}

<article class="blog-post">
    <header class="post-header">
        <h1>@Model.Content.Title.Value</h1>
        
        <div class="post-meta">
            <time datetime="@Model.Content.PublishDate.Value.ToString("yyyy-MM-dd")">
                发布于：@Model.Content.PublishDate.Value.ToString("yyyy年MM月dd日")
            </time>
            
            @if (Model.Content.Category != null)
            {
                <span class="category">
                    分类：@Model.Content.Category.DisplayValue
                </span>
            }
        </div>
    </header>

    @if (Model.Content.FeaturedImage != null)
    {
        <div class="featured-image">
            <img src="@Model.Content.FeaturedImage.Value" 
                 alt="@Model.Content.Title.Value"
                 loading="lazy">
        </div>
    }

    <div class="post-content markdown-body">
        @Html.Raw(Model.Content.Content.Html)
    </div>

    @if (Model.Content.Tags?.DisplayValues?.Any() == true)
    {
        <div class="post-tags">
            <strong>标签：</strong>
            @foreach (var tag in Model.Content.Tags.DisplayValues)
            {
                <span class="tag">@tag</span>
            }
        </div>
    }
</article>
```

### 2. 使用渲染服务

在控制器中使用 `IDocumentRenderService` 渲染内容：

```csharp
public class ContentController : Controller
{
    private readonly IDocumentRenderService _renderService;
    private readonly IContentRepository _contentRepository;

    public ContentController(
        IDocumentRenderService renderService,
        IContentRepository contentRepository)
    {
        _renderService = renderService;
        _contentRepository = contentRepository;
    }

    public async Task<IActionResult> ViewBySlug(string slug)
    {
        // 通过Slug渲染内容
        var html = await _renderService.RenderDocumentBySlugAsync(slug);
        if (string.IsNullOrEmpty(html))
        {
            return NotFound();
        }
        return Content(html, "text/html");
    }

    public async Task<IActionResult> ViewById(Guid id)
    {
        // 使用泛型方法渲染特定类型的内容
        var blogPost = await _contentRepository.GetContentAsync<BlogPost>(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        var html = await _renderService.RenderAsync(blogPost);
        return Content(html, "text/html");
    }
}
```

## 高级功能

### 1. 自定义渲染逻辑

您可以通过继承 `DefaultDocumentRenderService` 或实现 `IDocumentRenderService` 接口来自定义渲染逻辑：

```csharp
public class CustomDocumentRenderService : DefaultDocumentRenderService
{
    public CustomDocumentRenderService(
        IContentRepository contentRepository,
        IViewRenderService viewRenderService,
        IOptions<BiwenContentOptions> options,
        ILogger<CustomDocumentRenderService> logger)
        : base(contentRepository, viewRenderService, options, logger)
    {
    }

    protected override async Task<string> BeforeRenderAsync<T>(T content)
    {
        // 渲染前的处理逻辑
        return await base.BeforeRenderAsync(content);
    }

    protected override async Task<string> AfterRenderAsync<T>(T content, string renderedHtml)
    {
        // 渲染后的处理逻辑
        return await base.AfterRenderAsync(content, renderedHtml);
    }

    protected override string ResolveViewPath<T>(T content, string? viewName)
    {
        // 自定义视图路径解析逻辑
        if (viewName == null)
        {
            viewName = typeof(T).Name;
        }
        return $"Contents/{viewName}";
    }
}
```

## 配置与自定义

### 1. 配置选项

在 `Program.cs` 中配置内容渲染选项：

```csharp
builder.Services.AddBiwenContents<YourDbContext>(options => 
{
    // 设置视图路径
    options.ViewPath = "Views/Contents";
    
    // 配置渲染缓存
    options.EnableRenderCache = true;
    options.RenderCacheExpiration = TimeSpan.FromMinutes(10);
    
    // 配置视图查找规则
    options.ViewNameResolver = contentType => $"{contentType.Name}View";
});
```

### 2. 自定义布局

创建内容布局模板 `Views/Shared/_ContentLayout.cshtml`：

```cshtml
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"]</title>
    <link rel="stylesheet" href="~/css/content.css" />
    @await RenderSectionAsync("Styles", required: false)
</head>
<body>
    <div class="content-wrapper">
        <main role="main">
            @RenderBody()
        </main>
    </div>
    
    <script src="~/js/content.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### 3. 内容组件

创建可重用的内容组件：

```cshtml
@* Views/Shared/Components/_ContentMeta.cshtml *@
@model ContentViewModel<IContent>

<div class="content-meta">
    <div class="meta-item">
        <span class="meta-label">创建时间：</span>
        <time datetime="@Model.ContentDefine.CreateTime.ToString("yyyy-MM-dd")">
            @Model.ContentDefine.CreateTime.ToString("yyyy年MM月dd日")
        </time>
    </div>
    @if (Model.ContentDefine.UpdateTime != Model.ContentDefine.CreateTime)
    {
        <div class="meta-item">
            <span class="meta-label">更新时间：</span>
            <time datetime="@Model.ContentDefine.UpdateTime.ToString("yyyy-MM-dd")">
                @Model.ContentDefine.UpdateTime.ToString("yyyy年MM月dd日")
            </time>
        </div>
    }
    <div class="meta-item">
        <span class="meta-label">状态：</span>
        <span class="content-status @Model.ContentDefine.Status.ToString().ToLower()">
            @Model.ContentDefine.Status.GetDescription()
        </span>
    </div>
</div>
```
```

## 最佳实践与优化

### 1. 性能优化

#### 渲染缓存
```csharp
// 配置渲染缓存
services.AddBiwenContents<YourDbContext>(options => 
{
    options.EnableRenderCache = true;
    options.RenderCacheExpiration = TimeSpan.FromMinutes(10);
});

// 使用分布式缓存
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "BiwenContents:";
});
```

#### 资源优化
- 使用 `loading="lazy"` 延迟加载图片
- 启用响应式图片
```html
<img src="@Model.Content.Image.Value" 
     srcset="@Model.Content.Image.GetSrcSet()"
     sizes="(max-width: 768px) 100vw, 50vw"
     alt="@Model.Content.Title.Value" 
     loading="lazy">
```

#### 批量渲染
```csharp
public async Task<IActionResult> List(int page = 1)
{
    var contents = await _repository.GetContentsByTypeAsync<BlogPost>(
        pageIndex: page - 1,
        pageSize: 10
    );
    
    var tasks = contents.Items.Select(post => 
        _renderService.RenderAsync(post, "BlogPostSummary"));
    
    var renderedHtmls = await Task.WhenAll(tasks);
    return View(renderedHtmls);
}
```

### 2. SEO 优化

#### Meta 标签
```cshtml
@{
    ViewData["Title"] = Model.Content.Title.Value;
    ViewData["Description"] = Model.Content.Summary.Value;
    ViewData["Keywords"] = string.Join(",", Model.Content.Tags.DisplayValues);
}

<meta name="description" content="@ViewData["Description"]">
<meta name="keywords" content="@ViewData["Keywords"]">
<meta property="og:title" content="@ViewData["Title"]">
<meta property="og:description" content="@ViewData["Description"]">
@if (Model.Content.FeaturedImage != null)
{
    <meta property="og:image" content="@Model.Content.FeaturedImage.Value">
}
```

#### 结构化数据
```cshtml
<script type="application/ld+json">
{
    "@context": "https://schema.org",
    "@type": "Article",
    "headline": "@Model.Content.Title.Value",
    "datePublished": "@Model.ContentDefine.CreateTime.ToString("yyyy-MM-dd")",
    "dateModified": "@Model.ContentDefine.UpdateTime.ToString("yyyy-MM-dd")",
    "description": "@Model.Content.Summary.Value"
}
</script>
```

### 3. 安全性

#### XSS 防护
使用 HTML 安全库处理富文本内容：
```csharp
public class SafeHtmlConverter : IMarkdownConverter
{
    private readonly HtmlSanitizer _sanitizer;
    
    public SafeHtmlConverter()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Add("article");
        _sanitizer.AllowedTags.Add("section");
        // 配置允许的标签和属性
    }
    
    public string ToHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown);
        return _sanitizer.Sanitize(html);
    }
}
```

#### 访问控制
```csharp
public class ContentAuthorizationHandler : AuthorizationHandler<ContentRequirement, IContent>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ContentRequirement requirement,
        IContent content)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? false)
        {
            return Task.CompletedTask;
        }

        if (content.IsPublic || user.IsInRole("Editor"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### 4. 错误处理

```csharp
public class ContentController : Controller
{
    private readonly IDocumentRenderService _renderService;
    private readonly ILogger<ContentController> _logger;

    public async Task<IActionResult> View(string slug)
    {
        try
        {
            var html = await _renderService.RenderDocumentBySlugAsync(slug);
            if (string.IsNullOrEmpty(html))
            {
                return View("NotFound", new ErrorViewModel 
                { 
                    Message = "未找到指定内容",
                    Slug = slug
                });
            }
            return Content(html, "text/html");
        }
        catch (ContentNotFoundException ex)
        {
            _logger.LogWarning(ex, "内容不存在: {Slug}", slug);
            return View("NotFound");
        }
        catch (RenderException ex)
        {
            _logger.LogError(ex, "渲染内容失败: {Slug}", slug);
            return View("Error", new ErrorViewModel 
            { 
                Message = "渲染内容时发生错误",
                Details = ex.Message
            });
        }
    }
}
```

### 5. 监控与诊断

```csharp
public class ContentRenderingDiagnostics
{
    private readonly ILogger<ContentRenderingDiagnostics> _logger;
    private readonly IMetricsCollector _metrics;

    public async Task<string> RenderWithDiagnosticsAsync<T>(
        T content,
        Func<T, Task<string>> renderFunc) where T : IContent
    {
        using var scope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["ContentId"] = content.Id,
                ["ContentType"] = typeof(T).Name
            });

        var sw = Stopwatch.StartNew();
        try
        {
            var html = await renderFunc(content);
            sw.Stop();

            _metrics.RecordRenderingTime(
                typeof(T).Name,
                sw.ElapsedMilliseconds);

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "渲染失败");
            _metrics.IncrementRenderingErrors(typeof(T).Name);
            throw;
        }
    }
}
```

## 相关链接

- [内容类型定义](ContentTypes.md)：了解如何定义内容类型
- [字段类型](FieldTypes.md)：了解可用的字段类型
- [版本控制](Versioning.md)：了解内容版本控制功能
- [API集成](ApiIntegration.md)：了解如何通过API操作内容
