// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 19:01:59 ContentAuditLog.cs

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.Contents.Domain;

/// <summary>
/// 内容审计日志
/// </summary>
[Index(nameof(ContentAuditLog.ContentId), nameof(ContentAuditLog.Timestamp))]
public class ContentAuditLog
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
    /// Represents the name of the operator, defaulting to '系统'. It can be null.
    /// </summary>
    public string? OperatorName { get; set; } = "系统";

    /// <summary>
    /// 操作类型
    /// </summary>
    [Required, StringLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 操作详情
    /// </summary>
    [Required]
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// 操作者ID
    /// </summary>
    [StringLength(100)]
    public string? OperatorId { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 关联的内容
    /// </summary>
    public virtual Content? Content { get; set; }
}
