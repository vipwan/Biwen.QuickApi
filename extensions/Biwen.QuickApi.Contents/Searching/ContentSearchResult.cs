// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-30 17:19:52 ContentSearchResult.cs

using Biwen.QuickApi.Contents.Domain;

namespace Biwen.QuickApi.Contents.Searching;

/// <summary>
/// 搜索结果DTO，专门用于从Elasticsearch返回
/// </summary>
public class ContentSearchResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ContentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ContentType { get; set; } = string.Empty;

    // JsonContent作为对象列表
    public List<ContentFieldValue> JsonContent { get; set; } = new();
}