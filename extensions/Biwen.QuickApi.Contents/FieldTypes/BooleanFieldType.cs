// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:12:07 BooleanFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 布尔字段实现
/// </summary>
public class BooleanFieldType : IFieldType
{
    public string Name => "布尔值";
    public string SystemName => "boolean";
    public Type ValueType => typeof(bool);

    // 添加带参构造函数
    public BooleanFieldType() { }
    public BooleanFieldType(bool value)
    {
        Value = value.ToString().ToLower();
    }



    public object? ConvertValue(string value)
    {
        if (bool.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is bool boolValue)
        {
            return boolValue.ToString();
        }
        else if (value is string stringValue)
        {
            return stringValue;//true,false;
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return bool.TryParse(value, out _);
    }

    private object? _value;

    /// <summary>
    /// 值属性.注意返回bool值的小写形式
    /// </summary>
    public string Value
    {
        get => _value?.ToString()?.ToLower() ?? string.Empty;
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
    public string? GetValidationErrorMessage() => "请输入有效的布尔值";
}