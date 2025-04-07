// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 17:11:56 IFieldType.cs

namespace Biwen.QuickApi.Contents.Abstractions;

/// <summary>
/// 字段类型接口，所有字段类型必须实现此接口
/// </summary>
public interface IFieldType
{
    /// <summary>
    /// 字段名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 字段系统名称
    /// </summary>
    string SystemName { get; }

    /// <summary>
    /// 值类型
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// 值属性
    /// </summary>
    string Value { get; set; }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    object? GetValue();

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    void SetValue(object? value);

    /// <summary>
    /// 验证字段值
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <param name="rules">验证规则</param>
    /// <returns>验证结果</returns>
    bool Validate(string value, string? rules = null);

    /// <summary>
    /// 转换值为对象
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <returns>转换后的对象</returns>
    object? ConvertValue(string value);

    /// <summary>
    /// 转换对象为字符串
    /// </summary>
    /// <param name="value">对象值</param>
    /// <returns>转换后的字符串</returns>
    string ConvertToString(object? value);

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    string? GetValidationErrorMessage();
}

/// <summary>
/// 使用扩展方法来简化字段类型的使用
/// </summary>
public static class FieldTypeExtensions
{
    /// <summary>
    /// 获取存储的字符串值
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static string GetValueString(this IFieldType field)
    {
        return field.Value;
    }
}