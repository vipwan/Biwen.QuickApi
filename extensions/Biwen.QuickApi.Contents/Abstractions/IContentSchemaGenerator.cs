// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 17:11:56 IContentSchemaGenerator.cs

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Biwen.QuickApi.Contents.Abstractions;

/// <summary>
/// 内容Schema生成器接口
/// </summary>
public interface IContentSchemaGenerator
{
    /// <summary>
    /// 生成指定内容类型的Schema
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <returns>Schema对象</returns>
    JsonObject GenerateSchema<T>() where T : IContent;

    /// <summary>
    /// 生成指定内容类型的Schema
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <returns>Schema对象</returns>
    JsonObject GenerateSchema(Type contentType);

    /// <summary>
    /// 生成指定内容类型的Schema JSON字符串
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="options">JSON序列化选项</param>
    /// <returns>Schema JSON字符串</returns>
    string GenerateSchemaJson<T>(JsonSerializerOptions? options = null) where T : IContent;

    /// <summary>
    /// 生成指定内容类型的Schema JSON字符串
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <param name="options">JSON序列化选项</param>
    /// <returns>Schema JSON字符串</returns>
    string GenerateSchemaJson(Type contentType, JsonSerializerOptions? options = null);
} 