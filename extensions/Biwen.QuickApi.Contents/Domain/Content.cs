// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 15:03:50 ContentTypeField.cs

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.Contents.Domain;

/// <summary>
/// 持久层实体
/// </summary>
[Index(
    nameof(Content.Slug),
    nameof(Content.Status),
    nameof(Content.ContentType))]
public class Content
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Slug { get; set; } = string.Empty;

    public ContentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required, StringLength(maximumLength: 1024)]
    public string ContentType { get; set; } = string.Empty; // 存储内容类型的完全限定名

    [Required]
    public string JsonContent { get; set; } = string.Empty; // 存储序列化后的内容字段值
}

public enum ContentStatus
{
    [Description("草稿")]
    Draft,
    [Description("已发布")]
    Published,
    [Description("存档")]
    Archived
}