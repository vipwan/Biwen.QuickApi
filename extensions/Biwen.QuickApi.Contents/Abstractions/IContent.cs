// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 17:11:56 IContent.cs

using System.Reflection;
using System.Text.Json.Serialization;

namespace Biwen.QuickApi.Contents.Abstractions;

/// <summary>
/// 内容接口，所有内容类型必须实现此接口
/// </summary>
public interface IContent
{
    /// <summary>
    /// 文档类型
    /// </summary>
    [JsonIgnore]
    string Content_ContentType { get; }

    /// <summary>
    /// 文档描述
    /// </summary>
    [JsonIgnore]
    string Content_Description { get; }

    /// <summary>
    /// 排序
    /// </summary>
    [JsonIgnore]
    int Content_Order { get; }

}


/// <summary>
/// 文档类型的抽象类,默认的文档类型名为:typeof(<typeparamref name="T"/>).FullName
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ContentBase<T> : IContent
{
    /// <summary>
    /// 文档类型
    /// </summary>
    [JsonIgnore]
    public virtual string Content_ContentType { get; } = typeof(T).FullName!;

    /// <summary>
    /// 文档描述
    /// </summary>
    [JsonIgnore]
    public virtual string Content_Description
    {
        get
        {
            var attr = typeof(T).GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? typeof(T).Name;
        }
    }

    /// <summary>
    /// 排序
    /// </summary>
    [JsonIgnore]
    public virtual int Content_Order { get; } = 1000;
}