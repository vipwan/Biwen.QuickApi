# 内容类型定义

内容类型是 CMS 的核心概念，它定义了内容的结构和行为。本文将介绍如何定义和管理内容类型。

## 基本定义

### 1. 创建内容类型

创建一个继承自 `ContentBase<T>` 的类来定义内容类型：

```csharp
[Description("文章")]
public class Article : ContentBase<Article>
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    public TextFieldType Title { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType? Content { get; set; }

    /// <summary>
    /// 发布日期
    /// </summary>
    [Display(Name = "发布日期")]
    public DateTimeFieldType? PublishDate { get; set; }

    /// <summary>
    /// 是否发布
    /// </summary>
    [Display(Name = "是否发布")]
    public BooleanFieldType IsPublished { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [Display(Name = "作者")]
    public TextFieldType? Author { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [Display(Name = "标签")]
    [ArrayField(10, 5)]
    public ArrayFieldType? Tags { get; set; }

    public override string Content_Description => "文章内容";
}
```

### 2. 字段类型

Biwen.QuickApi.Contents 提供了多种字段类型：

- `TextFieldType`: 文本字段
- `TextAreaFieldType`: 多行文本字段
- `MarkdownFieldType`: Markdown 编辑器字段
- `NumberFieldType`: 数字字段
- `IntegerFieldType`: 整数字段
- `BooleanFieldType`: 布尔字段
- `DateTimeFieldType`: 日期时间字段
- `TimeFieldType`: 时间字段
- `UrlFieldType`: URL 字段
- `ColorFieldType`: 颜色字段
- `FileFieldType`: 文件字段
- `ImageFieldType`: 图片字段
- `OptionsFieldType`: 单选字段
- `OptionsMultiFieldType`: 多选字段
- `ArrayFieldType`: 数组字段

## 高级定义

### 1. 继承关系

内容类型可以继承其他内容类型：

```csharp
[Description("博客文章")]
public class BlogPost : Article
{
    /// <summary>
    /// 分类
    /// </summary>
    [Display(Name = "分类")]
    [OptionsField("技术", "生活", "其他")]
    public OptionsFieldType? Category { get; set; }

    /// <summary>
    /// 封面图片
    /// </summary>
    [Display(Name = "封面图片")]
    public ImageFieldType? CoverImage { get; set; }
}
```

### 2. 字段属性

使用特性来定义字段的属性：

```csharp
[Display(Name = "标题")]  // 显示名称
[Required]               // 必填
[MaxLength(100)]        // 最大长度
[Description("文章标题")] // 描述
[MarkdownToolBar(MarkdownToolStyle.Standard)] // Markdown 工具栏样式
[ArrayField(10, 5)]     // 数组字段限制
[OptionsField("选项1", "选项2")] // 选项字段
```

## 使用方式

### 1. 内容创建

```csharp
public class ArticleService
{
    private readonly IContentService _contentService;

    public async Task<Article> CreateArticleAsync(CreateArticleDto dto)
    {
        var article = new Article
        {
            Title = new TextFieldType { Value = dto.Title },
            Content = new MarkdownFieldType { Value = dto.Content },
            PublishDate = new DateTimeFieldType { Value = DateTime.Now },
            Author = new TextFieldType { Value = dto.Author },
            Tags = new ArrayFieldType { Values = dto.Tags }
        };
        await _contentService.CreateAsync(article);
        return article;
    }
}
```

### 2. 内容查询

```csharp
public class ArticleQueryService
{
    private readonly IContentService _contentService;

    public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
    {
        return await _contentService.Query<Article>()
            .Where(x => x.IsPublished.Value)
            .OrderByDescending(x => x.PublishDate.Value)
            .ToListAsync();
    }
}
```

## 最佳实践

1. **字段设计**
   - 使用合适的字段类型
   - 添加必要的验证规则
   - 提供清晰的字段描述
   - 考虑字段的扩展性

2. **类型设计**
   - 保持类型职责单一
   - 合理使用继承
   - 考虑性能影响
   - 提供合适的默认值

3. **验证规则**
   - 使用字段类型内置的验证
   - 提供清晰的错误信息
   - 考虑业务规则

## 示例

### 1. 产品内容类型

```csharp
[Description("产品")]
public class Product : ContentBase<Product>
{
    [Display(Name = "产品名称")]
    [Required]
    public TextFieldType Name { get; set; } = null!;

    [Display(Name = "产品描述")]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType? Description { get; set; }

    [Display(Name = "价格")]
    [Required]
    public NumberFieldType Price { get; set; } = null!;

    [Display(Name = "分类")]
    [OptionsMultiField("电子产品", "家居用品", "服装")]
    public OptionsMultiFieldType? Categories { get; set; }

    [Display(Name = "规格")]
    [ArrayField(10, 5)]
    public ArrayFieldType? Specifications { get; set; }

    [Display(Name = "产品图片")]
    public ImageFieldType? ProductImage { get; set; }

    public override string Content_Description => "产品信息";
}
```

### 2. 页面内容类型

```csharp
[Description("页面")]
public class Page : ContentBase<Page>
{
    [Display(Name = "标题")]
    [Required]
    public TextFieldType Title { get; set; } = null!;

    [Display(Name = "URL别名")]
    public TextFieldType? Slug { get; set; }

    [Display(Name = "内容")]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType? Content { get; set; }

    [Display(Name = "Meta描述")]
    public TextAreaFieldType? MetaDescription { get; set; }

    [Display(Name = "关键词")]
    [ArrayField(10, 5)]
    public ArrayFieldType? Keywords { get; set; }

    public override string Content_Description => "页面内容";
}
```

## 下一步

- [字段类型](FieldTypes.md)
- [内容渲染](ContentRendering.md)
- [版本控制](Versioning.md)
