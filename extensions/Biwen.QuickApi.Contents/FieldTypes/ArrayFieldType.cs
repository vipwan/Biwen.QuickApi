// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 00:11:56 ArrayFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 数组字段实现（字符串数组）使用tag
/// </summary>
public class ArrayFieldType : IFieldType
{
    public string Name => "字符串数组";
    public string SystemName => "tags";
    public Type ValueType => typeof(string[]);


    const string SplitString = ",";

    public object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(SplitString, StringSplitOptions.RemoveEmptyEntries);
    }

    public string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string[] array)
        {
            return string.Join(SplitString, array);
        }

        return value.ToString() ?? string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        // 字符串数组不需要特殊验证
        return true;
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
    public string? GetValidationErrorMessage() => null;
}


/// <summary>
/// 标记数组字段的属性
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ArrayFieldAttribute(int maxLength = 20, int maxCount = 10) : Attribute
{
    /// <summary>
    /// 默认单个元素最大长度
    /// </summary>
    public int MaxLength { get; set; } = maxLength;

    /// <summary>
    /// 默认最大数量
    /// </summary>
    public int MaxCount { get; set; } = maxCount;
}