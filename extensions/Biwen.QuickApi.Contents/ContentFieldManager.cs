// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-05 01:02:14 ContentFieldManager.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents;

/// <summary>
/// Manages a collection of field types indexed by their system names. Provides methods to retrieve a specific field
/// type or all field types.
/// </summary>
public class ContentFieldManager
{
    private readonly Dictionary<string, IFieldType> _fieldTypes = [];

    public ContentFieldManager(IEnumerable<IFieldType> fieldTypes)
    {
        foreach (var fieldType in fieldTypes)
        {
            _fieldTypes[fieldType.SystemName] = fieldType;
        }
    }

    public IFieldType? GetFieldType(string systemName)
    {
        return _fieldTypes.TryGetValue(systemName, out var fieldType) ? fieldType : null;
    }

    /// <summary>
    /// 获取所有注册的字段类型
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IFieldType> GetAllFieldTypes()
    {
        return _fieldTypes.Values;
    }
}