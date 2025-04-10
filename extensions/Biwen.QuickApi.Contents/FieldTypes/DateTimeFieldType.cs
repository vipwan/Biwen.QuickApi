// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:17 DateTimeFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 日期时间字段实现
/// </summary>
public class DateTimeFieldType : IFieldType
{
    public string Name => "日期时间";
    public string SystemName => "datetime";
    public Type ValueType => typeof(DateTime);

    // 添加带参构造函数
    public DateTimeFieldType() { }
    public DateTimeFieldType(DateTime value)
    {
        Value = value.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public object? ConvertValue(string value)
    {
        if (DateTime.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return DateTime.TryParse(value, out _);
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
    public string? GetValidationErrorMessage() => "请输入有效的日期时间";
}