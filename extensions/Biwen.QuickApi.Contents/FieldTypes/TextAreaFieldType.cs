﻿// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:53 TextAreaFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// TextArea字段实现
/// </summary>
public class TextAreaFieldType : IFieldType
{
    public string Name => "TextArea";
    public string SystemName => "textArea";
    public Type ValueType => typeof(string);

    // 添加带参构造函数
    public TextAreaFieldType() { }

    public TextAreaFieldType(string value)
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