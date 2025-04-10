# 字段类型

Biwen.QuickApi 的 CMS 模块提供了丰富的字段类型系统，支持各种内容展示和输入需求。所有字段类型都实现了 `IFieldType` 接口。

## 基本字段类型

### 1. 文本字段 (TextFieldType)

用于存储普通文本内容：

```csharp
[Display(Name = "标题")]
[Required]
public TextFieldType Title { get; set; } = null!;
```

### 2. 多行文本字段 (TextAreaFieldType)

用于存储多行文本内容：

```csharp
[Display(Name = "描述")]
public TextAreaFieldType? Description { get; set; }
```

### 3. Markdown字段 (MarkdownFieldType)

用于存储 Markdown 格式的内容：

```csharp
[Display(Name = "内容")]
[MarkdownToolBar(MarkdownToolStyle.Standard)]
public MarkdownFieldType? Content { get; set; }
```

### 4. 数字字段 (NumberFieldType)

用于存储数值类型：

```csharp
[Display(Name = "价格")]
[Required]
public NumberFieldType Price { get; set; } = null!;
```

### 5. 整数字段 (IntegerFieldType)

用于存储整数值：

```csharp
[Display(Name = "数量")]
[Required]
public IntegerFieldType Quantity { get; set; } = null!;
```

### 6. 布尔字段 (BooleanFieldType)

用于存储布尔值：

```csharp
[Display(Name = "是否发布")]
public BooleanFieldType IsPublished { get; set; }
```

### 7. 日期时间字段 (DateTimeFieldType)

用于存储日期时间：

```csharp
[Display(Name = "发布日期")]
public DateTimeFieldType? PublishDate { get; set; }
```

### 8. 时间字段 (TimeFieldType)

用于存储时间：

```csharp
[Display(Name = "发布时间")]
public TimeFieldType? PublishTime { get; set; }
```

## 高级字段类型

### 1. 选项字段 (OptionsFieldType)

用于单选：

```csharp
[Display(Name = "分类")]
[OptionsField("技术", "生活", "其他")]
public OptionsFieldType? Category { get; set; }
```

### 2. 多选字段 (OptionsMultiFieldType)

用于多选：

```csharp
[Display(Name = "标签")]
[OptionsMultiField("C#", "ASP.NET", "JavaScript")]
public OptionsMultiFieldType? Tags { get; set; }
```

### 3. 数组字段 (ArrayFieldType)

用于存储数组：

```csharp
[Display(Name = "图片集")]
[ArrayField(10, 5)]
public ArrayFieldType? Gallery { get; set; }
```

### 4. 图片字段 (ImageFieldType)

用于存储图片：

```csharp
[Display(Name = "封面图片")]
public ImageFieldType? CoverImage { get; set; }
```

### 5. 文件字段 (FileFieldType)

用于存储文件：

```csharp
[Display(Name = "附件")]
public FileFieldType? Attachment { get; set; }
```

### 6. URL字段 (UrlFieldType)

用于存储 URL：

```csharp
[Display(Name = "链接")]
public UrlFieldType? Link { get; set; }
```

### 7. 颜色字段 (ColorFieldType)

用于存储颜色：

```csharp
[Display(Name = "主题色")]
public ColorFieldType? ThemeColor { get; set; }
```

## 字段属性

### 1. 显示名称

```csharp
[Display(Name = "标题")]
```

### 2. 必填验证

```csharp
[Required]
```

### 3. Markdown 工具栏样式

```csharp
[MarkdownToolBar(MarkdownToolStyle.Standard)]
```

### 4. 数组字段限制

```csharp
[ArrayField(10, 5)] // 最大10个元素，最小5个元素
```

### 5. 选项字段

```csharp
[OptionsField("选项1", "选项2", "选项3")]
[OptionsMultiField("选项1", "选项2", "选项3")]
```

## 最佳实践

1. **字段选择**
   - 根据数据类型选择合适的字段类型
   - 考虑字段的展示和编辑需求
   - 评估字段的性能影响

2. **字段属性**
   - 添加必要的显示名称
   - 设置合适的验证规则
   - 提供清晰的字段描述

3. **性能优化**
   - 避免使用过大的字段类型
   - 合理使用索引
   - 考虑缓存策略

## 示例

### 1. 文章字段定义

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

    [Display(Name = "封面图片")]
    public ImageFieldType? CoverImage { get; set; }

    [Display(Name = "分类")]
    [OptionsField("技术", "生活", "其他")]
    public OptionsFieldType? Category { get; set; }

    [Display(Name = "标签")]
    [ArrayField(10, 5)]
    public ArrayFieldType? Tags { get; set; }

    public override string Content_Description => "文章内容";
}
```

### 2. 产品字段定义

```csharp
[Description("产品")]
public class Product : ContentBase<Product>
{
    [Display(Name = "产品名称")]
    [Required]
    public TextFieldType Name { get; set; } = null!;

    [Display(Name = "价格")]
    [Required]
    public NumberFieldType Price { get; set; } = null!;

    [Display(Name = "产品图片")]
    [ArrayField(5, 1)]
    public ArrayFieldType? Images { get; set; }

    [Display(Name = "规格")]
    [ArrayField(10, 5)]
    public ArrayFieldType? Specifications { get; set; }

    public override string Content_Description => "产品信息";
}
```

## 下一步

- [内容类型定义](ContentTypes.md)
- [内容渲染](ContentRendering.md)
- [版本控制](Versioning.md)
