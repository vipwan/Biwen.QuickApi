# Biwen.QuickApi.Contents

Biwen.QuickApi.Contents是基于`Biwen.QuickApi`用于内容管理的库，可以帮助您轻松地在项目中添加内容管理系统(CMS)功能，支持不同类型的内容字段、内容版本控制、审计和渲染。

## 功能特性

- 灵活的内容模型定义
- 多种字段类型支持（文本、数字、日期、图片、文件等）
- 内容版本控制
- 内容审计日志
- 内容渲染服务
- 支持内容草稿和发布工作流

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
```

### 4. 定义内容模型

创建实现`IContent`接口的内容模型：

```csharp
public class BlogPost : IContent
{
    public TextFieldType Title { get; set; } = new();
    public UrlFieldType Slug { get; set; } = new();
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

### Q: 如何实现内容搜索？

A: 您可以扩展`ContentRepository`并添加基于全文搜索的方法，或者使用第三方搜索引擎如Elasticsearch。

## API 接口说明

Biwen.QuickApi.Contents 提供了一系列 API 接口，用于内容的增删改查等操作。所有 API 接口都位于 `/biwen/contents` 路径下（由 `Constants.GroupName` 定义）。

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
  "jsonContent": "序列化后的内容JSON字符串"
}
```

**字段说明：**
- `title` - 内容标题
- `slug` - 内容的 URL 友好标识符
- `contentType` - 内容类型的完全限定名或最后一个名称
- `jsonContent` - 序列化的内容，为 JSON 字符串

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
