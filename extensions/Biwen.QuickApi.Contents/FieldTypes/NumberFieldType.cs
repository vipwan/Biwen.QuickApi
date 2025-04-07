// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:42 NumberFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 数字字段实现,如果不设置Range,则范围0-100之间
/// </summary>
public class NumberFieldType : IFieldType
{
    public string Name => "数字";
    public string SystemName => "number";
    public Type ValueType => typeof(double);

    public object? ConvertValue(string value)
    {
        if (double.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is double doubleValue)
        {
            return doubleValue.ToString();
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return double.TryParse(value, out _);
    }

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
    public string? GetValidationErrorMessage() => "请输入有效的数字";
}