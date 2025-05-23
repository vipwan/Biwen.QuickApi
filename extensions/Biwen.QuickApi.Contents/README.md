﻿# Biwen.QuickApi.Contents

Biwen.QuickApi.Contents是基于`Biwen.QuickApi`用于内容管理的库，可以帮助您轻松地在项目中添加内容管理系统(CMS)功能，支持不同类型的内容字段、内容版本控制、审计和渲染。

## 功能特性

- 灵活的内容模型定义
- 多种字段类型支持（文本、数字、日期、图片、文件等）
- 内容版本控制
- 内容审计日志
- 内容渲染服务
- 支持内容草稿和发布工作流
- 基于Elasticsearch的全文搜索功能

## 快速入门

### 1. 安装

在您的项目中引用Biwen.QuickApi.Contents包：

```bash
dotnet add package Biwen.QuickApi.Contents
```

### 2. 配置数据库上下文

创建一个实现`IContentDbContext`接口的数据库上下文：

```csharp
public class YourDbContext : DbContext, IContentDbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
    {
    }
    
    public DbSet<Content> Contents { get; set; }
    public DbSet<ContentAuditLog> ContentAuditLogs { get; set; }
    public DbSet<ContentVersion> ContentVersions { get; set; }
    
    public DbContext Context => this;
}
```

### 3. 在Program.cs中注册服务

```csharp
// 配置Elasticsearch客户端（可选，如需使用搜索功能）
builder.Services.AddSingleton(sp =>
{
    return new ElasticsearchClient(new Uri(builder.Configuration["ElasticSearch"]!));
});
// 添加内容搜索服务（可选，如需使用搜索功能）
builder.Services.AddScoped<IContentSearchService, ElasticsearchService>();

// 添加BiweContents核心服务
builder.Services.AddBiwenContents<YourDbContext>(options => 
{
    // 是否启用Api接口
    options.EnableApi = true; // 默认启用
    // 是否生成OpenApi文档
    options.GenerateApiDocument = true; // 默认启用

    // 自定义配置
    options.ViewPath = "\\Views\\Contents"; // 设置内容视图路径
    options.PermissionValidator = httpContext => 
    {
        // 您的权限验证逻辑
        return Task.FromResult(httpContext.User.Identity!.IsAuthenticated);
    };
});

// 应用程序启动后初始化Elasticsearch索引（可选）
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var searchService = scope.ServiceProvider.GetService<IContentSearchService>();
    if (searchService != null)
    {
        await searchService.InitializeIndexAsync();
    }
});
```

### 4. 定义内容模型

创建继承于`ContentBase<T>`的内容模型：

```csharp
public class BlogPost : ContentBase<BlogPost>
{
    [Required]
    public TextFieldType Title { get; set; } = new();
    public UrlFieldType Slug { get; set; } = new();
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType Content { get; set; } = new();
    public DateTimeFieldType PublishDate { get; set; } = new();
    public ImageFieldType FeaturedImage { get; set; } = new();
    
    // 使用枚举类型作为选项
    public enum CategoryType
    {
        技术 = 1,
        新闻 = 2,
        生活 = 3
    }
    public OptionsFieldType<CategoryType> Category { get; set; } = new();
    
    // 多选项也使用枚举类型
    [Flags]
    public enum TagType
    {
        [Description(".NET")]
        DotNet = 1,
        CSharp = 2,
        AspNetCore = 4,
        Frontend = 8,
        Backend = 16
    }
    public OptionsMultiFieldType<TagType> Tags { get; set; } = new();
}
```

## 使用内容仓储

### 创建或更新内容

```csharp
public class BlogService
{
    private readonly IContentRepository _contentRepository;
    
    public BlogService(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }
    
    public async Task<Guid> CreateBlogPostAsync(BlogPost post)
    {
        // 创建内容并保存
        return await _contentRepository.SaveContentAsync(post, post.Title.Value, post.Slug.Value);
    }
    
    public async Task UpdateBlogPostAsync(Guid id, BlogPost post)
    {
        // 更新内容
        await _contentRepository.UpdateContentAsync(id, post);
    }
}
```

### 查询内容

```csharp
// 通过ID获取内容
var post = await _contentRepository.GetContentAsync<BlogPost>(id);

// 获取特定类型的所有内容分页列表
var pagedPosts = await _contentRepository.GetContentsByTypeAsync<BlogPost>(pageIndex: 0, len: 10);

// 通过slug获取内容
var postBySlug = await _contentRepository.GetContentsByTypeAsync<BlogPost>("my-blog-post");
```

### 发布内容

```csharp
// 设置内容状态为已发布
await _contentRepository.SetContentStatusAsync(id, ContentStatus.Published);
```

### 删除内容

```csharp
await _contentRepository.DeleteContentAsync(id);
```

## 内容字段类型

Biwen.QuickApi.Contents支持以下字段类型：

- `TextFieldType` - 短文本字段
- `TextAreaFieldType` - 长文本字段
- `MarkdownFieldType` - Markdown富文本
- `IntegerFieldType` - 整数
- `NumberFieldType` - 浮点数
- `BooleanFieldType` - 布尔值
- `DateTimeFieldType` - 日期时间
- `TimeFieldType` - 时间
- `UrlFieldType` - URL链接
- `ColorFieldType` - 颜色
- `ImageFieldType` - 图片
- `FileFieldType` - 文件
- `OptionsFieldType<T>` - 单选项(枚举)
- `OptionsMultiFieldType<T>` - 多选项(枚举)
- `ArrayFieldType` - 数组类型

## 内容渲染

您可以使用`IDocumentRenderService`来渲染内容：

```csharp
public class BlogController : Controller
{
    private readonly IContentRepository _contentRepository;
    private readonly IDocumentRenderService _renderService;
    
    public BlogController(
        IContentRepository contentRepository, 
        IDocumentRenderService renderService)
    {
        _contentRepository = contentRepository;
        _renderService = renderService;
    }
    
    public async Task<IActionResult> ViewBlogPost(string slug)
    {
        var post = await _contentRepository.GetContentsByTypeAsync<BlogPost>(slug);
        if (post == null)
            return NotFound();
            
        // 渲染内容，使用指定的视图模板
        var renderedContent = await _renderService.RenderAsync(post, "BlogPost");
        
        return View("BlogPost", renderedContent);
    }
}
```

## 自定义文档类型视图

Biwen.QuickApi.Contents 允许您为每种内容类型创建自定义视图模板，以控制内容的呈现方式。

### 视图位置规范

视图文件应遵循以下命名和位置规范：

1. **基础路径**：视图文件默认位于 `Views/Contents` 目录下（可通过 `BiwenContentOptions.ViewPath` 配置）
2. **视图命名**：视图名称应与内容类型的简单名称匹配，例如 `BlogPost.cshtml`
3. **子目录支持**：您可以在 `Views/Contents` 下创建子目录来组织不同类别的视图

### 创建自定义视图模板

以下是创建自定义视图模板的步骤：

1. 在您的项目中创建 `Views/Contents` 目录（如果不存在）
2. 创建与您的内容类型名称相匹配的 .cshtml 文件，例如 `BlogPost.cshtml` 
3. 在视图文件中添加必要的命名空间和模型指令

示例视图模板 `Views/Contents/BlogPost.cshtml`：

```cshtml
@model Biwen.QuickApi.Contents.Rendering.ContentViewModel<YourNamespace.BlogPost>
@{
    ViewData["Title"] = Model.Content.Title.Value;
    Layout = "_Layout"; // 使用您的布局文件
}

<div class="blog-post">
    <h1>@Model.Content.Title.Value</h1>
    
    <div class="metadata">
        <span class="date">发布于: @Model.Content.PublishDate.Value.ToString("yyyy-MM-dd")</span>
        <span class="category">分类: @Model.Content.Category.DisplayValue</span>
    </div>
    
    @if (Model.Content.FeaturedImage != null && !string.IsNullOrEmpty(Model.Content.FeaturedImage.Value))
    {
        <div class="featured-image">
            <img src="@Model.Content.FeaturedImage.Value" alt="@Model.Content.Title.Value" />
        </div>
    }
    
    <div class="content markdown-body">
        @Html.Raw(Model.Content.Content.Html)
    </div>
    
    @if (Model.Content.Tags != null && Model.Content.Tags.DisplayValues?.Any() == true)
    {
        <div class="tags">
            <strong>标签:</strong>
            @foreach (var tag in Model.Content.Tags.DisplayValues)
            {
                <span class="tag">@tag</span>
            }
        </div>
    }
</div>
```

### 视图模型结构

视图接收 `ContentViewModel<T>` 类型的模型，该模型包含两个主要属性：

- `Content`：您的强类型内容实例（如 `BlogPost`）
- `ContentDefine`：内容的元数据信息（ID、创建时间、状态等）

### 访问字段值

在视图中访问内容字段值的方法：

- 文本字段：`Model.Content.Title.Value`
- Markdown字段：`Html.Raw(Model.Content.Content.Html)`（已转换为HTML）
- 日期字段：`Model.Content.PublishDate.Value.ToString("yyyy-MM-dd")`
- 选项字段（单选）：`Model.Content.Category.DisplayValue`
- 选项字段（多选）：`Model.Content.Tags.DisplayValues`（字符串集合）

### 自定义CSS样式

为确保内容呈现一致，您可以引入特定的样式：

```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/markdown.css" />
    <link rel="stylesheet" href="~/css/blog-post.css" />
}
```

### 条件渲染

您可以根据内容的特定字段值进行条件渲染：

```cshtml
@if (Model.ContentDefine.Status == ContentStatus.Published)
{
    <div class="published-badge">已发布</div>
}
else if (Model.ContentDefine.Status == ContentStatus.Draft)
{
    <div class="draft-badge">草稿</div>
}
```

## 内容版本和审计

系统自动记录内容的变更历史：

```csharp
// 获取内容的版本历史
var versions = await _contentVersionService.GetVersionsAsync(contentId);

// 查看内容审计日志
var auditLogs = await _contentAuditLogService.GetAuditLogsAsync(contentId);
```

## 权限控制

您可以通过配置`BiwenContentOptions`中的`PermissionValidator`来控制对内容管理功能的访问权限：

```csharp
services.AddBiwenContents<YourDbContext>(options => 
{
    options.PermissionValidator = async httpContext => 
    {
        // 检查用户是否有管理员角色
        return httpContext.User.IsInRole("Admin");
    };
});
```

## 最佳实践

1. 为每种内容类型创建专门的内容模型
2. 使用内容版本控制来跟踪内容变更
3. 为不同的内容类型创建专门的视图模板
4. 利用内容状态工作流来管理发布流程
5. 使用内容事件来实现自定义业务逻辑

## 内容搜索服务

Biwen.QuickApi.Contents提供基于Elasticsearch的内容搜索服务，支持全文搜索、分面搜索、高亮显示等高级功能。

### 自动索引更新

内容模块集成了自动索引更新功能，当内容被创建、更新、删除或状态发生变化时，系统会自动更新ElasticSearch索引。这是通过事件处理机制实现的，即使在应用程序没有启用ElasticSearch服务的情况下也不会产生错误。

事件处理器支持以下操作：
- 当内容创建后，自动添加到索引
- 当内容更新后，自动更新索引
- 当内容删除后，自动从索引中移除
- 当内容状态变更为已归档时，自动从索引中移除

### 配置Elasticsearch服务

1. 在`appsettings.json`中添加Elasticsearch连接配置：

```json
{
  "ElasticSearch": "http://localhost:9200"
}
```

2. 在项目的启动配置中注册Elasticsearch客户端和搜索服务：

```csharp
// 添加Elasticsearch客户端
builder.Services.AddSingleton(sp =>
{
    return new ElasticsearchClient(new Uri(builder.Configuration["ElasticSearch"]!));
});

// 添加内容搜索服务
builder.Services.AddScoped<IContentSearchService, ElasticsearchService>();
```

3. 应用程序启动时初始化索引：

```csharp
// 在应用程序启动时初始化Elasticsearch索引
app.UseEndpoints(endpoints =>
{
    // 其他端点配置...
    
    // 异步初始化Elasticsearch索引
    var scope = app.ApplicationServices.CreateScope();
    var searchService = scope.ServiceProvider.GetRequiredService<IContentSearchService>();
    _ = searchService.InitializeIndexAsync();
});
```

### 使用内容搜索服务

```csharp
public class SearchController : Controller
{
    private readonly IContentSearchService _searchService;
    private readonly IContentRepository _contentRepository;
    
    public SearchController(IContentSearchService searchService, IContentRepository contentRepository)
    {
        _searchService = searchService;
        _contentRepository = contentRepository;
    }
    
    // 重建索引
    public async Task<IActionResult> RebuildIndex()
    {
        // 获取所有内容
        var allContents = await _contentRepository.GetAllContentsAsync();
        
        // 重建索引
        await _searchService.RebuildIndexAsync(allContents);
        return Ok("索引重建完成");
    }
    
    // 执行搜索
    public async Task<IActionResult> Search(string query, int page = 1, int size = 10, string filter = null, string sort = null)
    {
        // 执行搜索查询
        var results = await _searchService.SearchContentsAsync(
            query,
            page,
            size,
            filter,
            sort,
            enableHighlight: true,
            facets: ["contentType"] // 返回内容类型分面
        );
        
        return View(results);
    }
}
```

### 搜索功能特性

搜索服务支持以下功能：

1. **全文搜索**：支持跨字段的全文搜索，可以搜索内容标题、Slug以及内容字段的值。
2. **模糊搜索**：使用Elasticsearch的模糊匹配功能，容忍拼写错误。
3. **高亮显示**：自动高亮显示搜索结果中与查询相关的部分。
4. **分面搜索**：支持按内容类型、状态等属性进行分面聚合。
5. **排序和过滤**：支持按创建时间、更新时间、标题等字段排序，并支持多种过滤方式。

### 搜索查询语法

搜索服务支持多种查询和过滤语法：

- **基础搜索**：直接输入关键词，例如：`博客`
- **类型过滤**：按内容类型过滤，有两种格式：
  - `contentType:BlogPost`
  - `contentType = 'BlogPost'`
- **状态过滤**：按内容状态过滤，例如：`status:Published`
- **字段筛选**：按特定字段和值过滤，例如：`field:Category=技术文章`
- **数值范围查询**：按数值字段范围过滤，例如：`range:Price=gte:100,lte:200`
  - 支持的操作符：`gte`（大于等于）、`lte`（小于等于）、`gt`（大于）、`lt`（小于）
- **日期范围查询**：按日期字段范围过滤，例如：`daterange:EventDate=gte:2024-01-01,lte:2024-12-31`
  - 支持的操作符：`gte`（大于等于）、`lte`（小于等于）、`gt`（大于）、`lt`（小于）
- **布尔条件查询**：按布尔字段过滤，例如：`bool:IsActive=true`
- **排序**：支持多种排序选项：
  - `createdAt:asc` - 按创建时间升序
  - `createdAt:desc` - 按创建时间降序
  - `updatedAt:asc` - 按更新时间升序
  - `updatedAt:desc` - 按更新时间降序
  - `title:asc` - 按标题字母升序
  - `title:desc` - 按标题字母降序

### 嵌套字段查询示例

以下是一些嵌套字段查询的具体示例：

1. **数值范围查询**：
```csharp
// 查询价格在150到250之间的产品
var result = await _searchService.SearchContentsAsync("", filter: "range:Price=gte:150,lte:250");
```

2. **日期范围查询**：
```csharp
// 查询2024年上半年的活动
var result = await _searchService.SearchContentsAsync("", filter: "daterange:EventDate=gte:2024-01-01,lte:2024-06-30");
```

3. **布尔条件查询**：
```csharp
// 查询激活状态的产品
var result = await _searchService.SearchContentsAsync("", filter: "bool:IsActive=true");
```

4. **组合查询**：
```csharp
// 查询2024年上半年的激活状态产品，价格在100到200之间
var result = await _searchService.SearchContentsAsync("", 
    filter: "daterange:EventDate=gte:2024-01-01,lte:2024-06-30 AND bool:IsActive=true AND range:Price=gte:100,lte:200");
```

这些查询都支持嵌套字段，可以精确匹配 `JsonContent` 中的字段值。查询结果会自动过滤出符合所有条件的文档。

## 常见问题

### Q: 如何自定义内容字段类型？

A: 您可以创建实现`IFieldType`接口的自定义字段类型，并在服务注册时添加它：

```csharp
public class CustomFieldType : IFieldType
{
    // 实现接口方法
}

// 注册
services.AddSingleton<IFieldType, CustomFieldType>();
```

### Q: 如何处理内容关联关系？

A: 您可以使用自定义字段类型来存储关联ID，或者在内容模型中使用特殊的引用字段。

### Q: 如何优化Elasticsearch搜索性能？

A: 可以考虑以下几点：
1. 在生产环境中增加分片数和副本数
2. 为高频查询创建专用索引
3. 使用筛选而非查询来提高性能
4. 定期重建索引以优化存储

## 通过Slug访问内容

Biwen.QuickApi.Contents提供了一个便捷的方法，使您可以通过自定义URL格式访问内容。您可以在应用程序启动时配置这个功能：

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // 其他中间件配置...
    
    app.UseEndpoints(endpoints =>
    {
        // 添加通过Slug访问内容的路由
        // 这将创建形如 /{prefix}/{slug} 的路由
        endpoints.MapBiwenContentsBySlug("p"); // 使用"p"作为URL前缀
        
        // 其他路由配置...
    });
}
```

上面的配置将创建如下格式的URL：`/p/{slug}`，例如`/p/my-blog-post`。当用户访问这个URL时，系统会自动查找匹配的内容并使用`IDocumentRenderService`进行渲染。

您可以根据需要自定义前缀，例如使用"contents"、"articles"、"pages"等。

## API 接口说明

Biwen.QuickApi.Contents 提供了一系列 API 接口，用于内容的增删改查等操作。所有 API 接口都位于 `~/contents` 路径下（由 `Constants.GroupName` 定义）。

### 内容查询 API

```
GET ~~/contents/infopages
```

用于查询内容列表，支持分页和过滤。

**查询参数：**
- `pageNumber` - 页码，从1开始，默认：1
- `pageSize` - 分页大小，默认：10
- `contentType` - 文档类型，必填
- `slug` - Slug，如果提供则进行精确匹配
- `title` - 按标题过滤
- `status` - 状态过滤，0：草稿，1：已发布，2：归档

**示例请求：**
```
GET ~~/contents/infopages?contentType=BlogPost&pageNumber=1&pageSize=10&status=1
```

### 获取指定内容 API

```
GET ~~/contents/{id:guid}
```

根据 ID 获取指定的内容详情。

**路径参数：**
- `id` - 内容的 GUID

**示例请求：**
```
GET ~~/contents/5f9c7b4e-3c1a-4f5b-9d1a-6c2a5e34b7d9
```

### 创建内容 API

```
POST ~~/contents/create
```

创建新的内容。

**请求体：**
```json
{
  "title": "内容标题",
  "slug": "content-slug",
  "contentType": "BlogPost",
  "jsonContent": "[{\"fieldName\":\"Title\",\"value\":\"hello world\"}]"
}
```

**字段说明：**
- `title` - 内容标题
- `slug` - 内容的 URL 友好标识符
- `contentType` - 内容类型的完全限定名或最后一个名称
- `jsonContent` - 序列化的内容，为 JSON 字符串,格式为`[{ "fieldName": "字段名称", "value": "字段值" }]`

### 更新内容 API

```
PUT ~~/contents/update/{id:guid}
```

更新已有内容。

**路径参数：**
- `id` - 内容的 GUID

**请求体：**
```json
{
  "title": "更新后的标题",
  "slug": "updated-slug",
  "status": 1,
  "jsonContent": "更新后的序列化内容"
}
```

### 设置内容状态 API

```
PUT ~~/contents/status/{id:guid}
```

设置内容的状态，如发布或归档。

**路径参数：**
- `id` - 内容的 GUID

**请求体：**
```json
{
  "status": 1
}
```

**状态值：**
- `0` - 草稿
- `1` - 已发布
- `2` - 归档

### 删除内容 API

```
DELETE ~~/contents/{id:guid}
```

删除指定的内容。

**路径参数：**
- `id` - 内容的 GUID

### 预览内容 API

```
GET ~~/contents/preview/{id:guid}
```

预览指定内容，不改变内容状态。

**路径参数：**
- `id` - 内容的 GUID

**返回：**
- 返回HTML格式的内容预览

### 内容版本管理 API

#### 获取内容版本列表

```
GET ~~/contents/versions/{id:guid}
```

获取指定内容的所有历史版本。

**路径参数：**
- `id` - 内容的 GUID

**返回：**
- 返回内容版本列表，包含版本ID、创建时间和版本说明等信息

#### 获取指定版本内容

```
GET ~~/contents/versions/{id:guid}/{version:guid}
```

获取内容的特定历史版本。

**路径参数：**
- `id` - 内容的 GUID
- `version` - 版本的 GUID

**返回：**
- 返回指定版本的内容快照

#### 回滚内容版本

```
POST ~~/contents/versions/{id:guid}/rollback/{version:guid}
```

将内容回滚到指定的历史版本。

**路径参数：**
- `id` - 内容的 GUID
- `version` - 要回滚到的版本 GUID

**返回：**
- 操作成功返回 `true`

### 内容审计日志 API

#### 获取内容审计日志

```
GET ~~/contents/{id:guid}/auditlogs
```

获取指定内容的所有审计日志记录。

**路径参数：**
- `id` - 内容的 GUID

**返回：**
- 返回内容的所有审计日志记录，包含操作类型、操作时间和操作者等信息

#### 按时间范围查询审计日志

```
GET ~~/contents/auditlogs
```

按时间范围查询系统中的内容审计日志。

**查询参数：**
- `startTime` - 开始时间，默认为当天开始
- `endTime` - 结束时间，默认为当前时间
- `pageNumber` - 页码，从1开始
- `pageSize` - 每页记录数

**返回：**
- 返回符合条件的审计日志分页列表

## 更多资源

- 查看示例项目以获取更多用法示例
- 参考API文档获取详细信息
