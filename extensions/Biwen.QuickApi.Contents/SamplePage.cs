// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 17:31:59 SamplePage.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;

namespace Biwen.QuickApi.Contents;

[Description("普通页面")]
public class SamplePage : ContentBase<SamplePage>
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required]
    public TextFieldType Title { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    [MarkdownToolBar(MarkdownToolStyle.Simple)]
    public MarkdownFieldType? Description { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType? Content { get; set; }

    [Display(Name = "Tags")]
    [Description("标签")]
    [ArrayField(10, 5)]
    public ArrayFieldType? Tags { get; set; }

    public override string Content_Description => "默认文档";

    /// <summary>
    /// 默认Page排序为最前
    /// </summary>
    public override int Content_Order => -1;
}
