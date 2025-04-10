// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 02:12:28 OptionsMultiFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

public class OptionsMultiFieldType<T> : IFieldType
{
    public string Name => "多选项";
    public string SystemName => "checkboxes";
    public Type ValueType => typeof(List<string>);

    // 添加带参构造函数
    public OptionsMultiFieldType() { }
    public OptionsMultiFieldType(List<Enum> value)
    {
        Value = string.Join(",", Convert.ToInt32(value));
    }


    public object? ConvertValue(string value) => value.Split(',').ToList();
    public string ConvertToString(object? value) => string.Join(",", (List<string>)value!);
    public bool Validate(string value, string? rules = null) => true;
    private List<string>? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => throw new NotImplementedException();
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
    public void SetValue(object? value) => _value = (List<string>?)value;

    public string? GetValidationErrorMessage()
    {
        throw new NotImplementedException();
    }
}
