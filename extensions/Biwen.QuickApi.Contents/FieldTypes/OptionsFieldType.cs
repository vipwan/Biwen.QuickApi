// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:47 OptionsFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 枚举单选字段实现
/// </summary>
/// <typeparam name="T">枚举类型</typeparam>
public class OptionsFieldType<T> : IFieldType
{
    public string Name => $"枚举单选<{typeof(T).Name}>";
    public string SystemName => $"enum";
    public Type ValueType => typeof(T);

    // 添加带参构造函数
    public OptionsFieldType() { }
    public OptionsFieldType(T value)
    {
        Value = Convert.ToInt32(value).ToString();
    }

    public object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is T enumValue)
        {
            return Convert.ToInt32(enumValue).ToString();
        }

        return value.ToString() ?? string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        if (int.TryParse(value, out var intValue))
        {
            return Enum.IsDefined(typeof(T), intValue);
        }

        return false;
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
    public string? GetValidationErrorMessage() => $"请输入有效的{typeof(T).Name}枚举值";
}
