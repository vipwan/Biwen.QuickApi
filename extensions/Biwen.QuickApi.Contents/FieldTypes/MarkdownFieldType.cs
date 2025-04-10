// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:36 MarkdownFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// MarkDown字段实现
/// </summary>
public class MarkdownFieldType : IFieldType
{
    public string Name => "Markdown";
    public string SystemName => "markdown";
    public Type ValueType => typeof(string);

    // 添加带参构造函数
    public MarkdownFieldType() { }
    public MarkdownFieldType(string value)
    {
        Value = value;
    }


    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null) => true;

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}



/// <summary>
/// 标记Markdown工具栏样式的属性,包含简单,完整,自定义三种样式
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="toolStyle">工具栏样式</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class MarkdownToolBarAttribute(MarkdownToolStyle toolStyle = MarkdownToolStyle.Standard) : Attribute
{
    /// <summary>
    /// 工具栏样式
    /// </summary>
    public MarkdownToolStyle ToolStyle { get; set; } = toolStyle;

    /// <summary>
    /// 自定义样式
    /// </summary>
    public string CustomToolStyle { get; set; } = string.Empty;
}

/// <summary>
/// markdown 工具栏样式枚举
/// </summary>
public enum MarkdownToolStyle
{
    /// <summary>
    /// 简单样式
    /// </summary>
    Simple,

    /// <summary>
    /// 标准样式
    /// </summary>
    Standard,


    /// <summary>
    /// 完整样式
    /// </summary>
    Full,
    /// <summary>
    /// 自定义样式
    /// </summary>
    Custom
}