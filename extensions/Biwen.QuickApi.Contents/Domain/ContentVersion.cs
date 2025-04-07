// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.Contents.Domain;

/// <summary>
/// 内容版本
/// </summary>
[Index(nameof(ContentVersion.ContentId), nameof(ContentVersion.Version))]
public class ContentVersion
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 内容ID
    /// </summary>
    [Required]
    public Guid ContentId { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [Required]
    public int Version { get; set; }

    /// <summary>
    /// 内容快照
    /// </summary>
    [Required]
    public string Snapshot { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    [StringLength(100)]
    public string? CreatorId { get; set; }

    /// <summary>
    /// 创建者名称
    /// </summary>
    [StringLength(100)]
    public string? CreatorName { get; set; }

    /// <summary>
    /// 关联的内容
    /// </summary>
    public virtual Content? Content { get; set; }
}
