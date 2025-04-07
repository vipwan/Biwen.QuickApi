// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 21:29:39 ContentSerializer.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biwen.QuickApi.Contents;

/// <summary>
/// 文档序列化器
/// </summary>
public class ContentSerializer
{
    private readonly ContentFieldManager _fieldManager;
    private readonly JsonSerializerOptions _options;

    public ContentSerializer(ContentFieldManager fieldManager)
    {
        _fieldManager = fieldManager;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }


    // 缓存所有的Content类型
    private static IEnumerable<IContent>? _contentTypesCache;
    private static readonly Lock _cacheLock = new();

    /// <summary>
    /// 返回所有的文档类型
    /// </summary>
    public IEnumerable<IContent> GetAllContentTypes()
    {
        if (_contentTypesCache != null)
            return _contentTypesCache;

        lock (_cacheLock)
        {
            if (_contentTypesCache != null)
                return _contentTypesCache;

            //使用反射获取所有实现了IContent接口的类型
            var contentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IContent).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (IContent)Activator.CreateInstance(t)!)
                .OrderBy(x => x.Content_Order)
                .ToList();

            _contentTypesCache = contentTypes;
            return _contentTypesCache;
        }
    }


    public string SerializeContent<T>(T content) where T : IContent
    {
        var fieldValues = new List<ContentFieldValue>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            // 只处理IFieldType类型的属性
            if (!typeof(IFieldType).IsAssignableFrom(property.PropertyType))
                continue;

            var fieldInstance = property.GetValue(content) as IFieldType;
            if (fieldInstance == null)
                continue;

            // 获取值字符串表示
            var value = GetFieldStringValue(fieldInstance);

            fieldValues.Add(new ContentFieldValue
            {
                FieldName = property.Name,
                Value = value
            });
        }

        return JsonSerializer.Serialize(fieldValues, _options);
    }

    public T DeserializeContent<T>(string json) where T : IContent, new()
    {
        //[{"fieldName":"Q","value":"5435435"},{"fieldName":"A","value":"Hello"},{"fieldName":"BackColor","value":"#431919"},{"fieldName":"IsTop","value":true},{"fieldName":"Category","value":"2"}]
        var content = new T();

        // 反序列化JSON字符串为ContentFieldValue列表
        var fieldValues = JsonSerializer.Deserialize<List<ContentFieldValue>>(json, _options);

        if (fieldValues == null)
            return content;

        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            // 只处理IFieldType类型的属性
            if (!typeof(IFieldType).IsAssignableFrom(property.PropertyType))
                continue;

            var fieldValue = fieldValues.FirstOrDefault(f => f.FieldName == property.Name);
            if (fieldValue == null)
                continue;

            // 创建字段类型实例
            var fieldTypeInstance = CreateFieldTypeInstance(property.PropertyType);
            if (fieldTypeInstance == null)
                continue;

            // 设置字段值
            SetFieldValue(fieldTypeInstance, fieldValue.Value);

            // 泛型则表示枚举,单选:
            if (property.PropertyType.IsGenericType && property.PropertyType.Name == (typeof(OptionsFieldType<>).Name))
            {
                // 获取枚举类型
                var enumType = property.PropertyType.GetGenericArguments()[0];
                // 将字段值转换为枚举值
                var enumValue = Enum.Parse(enumType, fieldValue.Value);
                // 构造对应的泛型OptionsFieldType<>
                var optionsFieldType = typeof(OptionsFieldType<>).MakeGenericType(enumType);
                // 设置属性值
                var optionsValue = Activator.CreateInstance(optionsFieldType);
                //调用SetValue方法
                var setValueMethod = optionsFieldType.GetMethod(nameof(IFieldType.SetValue));
                setValueMethod?.Invoke(optionsValue, [enumValue]);
                // 设置属性值
                property.SetValue(content, optionsValue);
            }
            // 如果是多选项
            else if (property.PropertyType.IsGenericType && property.PropertyType.Name == (typeof(OptionsMultiFieldType<>).Name))
            {
                // 获取枚举类型
                var enumType = property.PropertyType.GetGenericArguments()[0];
                //泛型多选项使用1,2,3逗号隔开,需要特殊处理:
                var values = fieldValue.Value.Split(',').ToList();//不需要转换.存储字符串数组
                // 构造对应的泛型OptionsFieldType<>
                var optionsFieldType = typeof(OptionsMultiFieldType<>).MakeGenericType(enumType);
                // 设置属性值
                var optionsValue = Activator.CreateInstance(optionsFieldType);
                //调用SetValue方法
                var setValueMethod = optionsFieldType.GetMethod(nameof(IFieldType.SetValue));
                setValueMethod?.Invoke(optionsValue, [values]);
                // 设置属性值
                property.SetValue(content, optionsValue);
            }
            else if (fieldTypeInstance is ArrayFieldType arrayFieldType)
            {
                // 修复：正确设置数组字段值，传入字段值而非实例本身
                arrayFieldType.SetValue(fieldValue.Value);
                // 设置属性值
                property.SetValue(content, arrayFieldType);
            }
            else if (fieldTypeInstance is IFieldType field)
            {
                // 设置属性值
                property.SetValue(content, field);
            }
            else
            {
                // 设置属性值
                property.SetValue(content, fieldTypeInstance);
            }
        }

        return content;
    }

    // 获取字段的字符串值
    // 添加缓存
    private readonly Dictionary<Type, FieldInfo?> _valueFieldCache = new();
    private readonly Dictionary<Type, PropertyInfo?> _valuePropertyCache = new();

    // 获取字段的字符串值
    private string GetFieldStringValue(IFieldType fieldInstance)
    {
        var type = fieldInstance.GetType();

        // 尝试从缓存获取
        if (!_valueFieldCache.TryGetValue(type, out var valueField))
        {
            valueField = type.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
            _valueFieldCache[type] = valueField;
        }

        if (valueField != null)
        {
            var value = valueField.GetValue(fieldInstance);
            return fieldInstance.ConvertToString(value);
        }

        // 尝试从缓存获取属性
        if (!_valuePropertyCache.TryGetValue(type, out var valueProperty))
        {
            valueProperty = type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _valuePropertyCache[type] = valueProperty;
        }

        if (valueProperty != null)
        {
            var value = valueProperty.GetValue(fieldInstance);
            return fieldInstance.ConvertToString(value);
        }

        return string.Empty;
    }
    // 设置字段值
    private void SetFieldValue(IFieldType fieldInstance, string value)
    {
        var convertedValue = fieldInstance.ConvertValue(value);

        // 使用反射设置私有的Value字段
        var valueField = fieldInstance.GetType().GetField("_value",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (valueField != null)
        {
            valueField.SetValue(fieldInstance, convertedValue);
            return;
        }

        // 如果没有_value字段，尝试通过属性设置
        var valueProperty = fieldInstance.GetType().GetProperty("Value",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (valueProperty != null)
        {
            valueProperty.SetValue(fieldInstance, convertedValue);
        }
    }

    // 创建字段类型实例
    private IFieldType? CreateFieldTypeInstance(Type fieldType)
    {
        // 根据字段类型创建实例
        var fieldSystemName = GetFieldTypeSystemName(fieldType);
        if (string.IsNullOrEmpty(fieldSystemName))
            return null;

        // 为每个属性创建新的实例，而不是重用FieldManager中的单例
        IFieldType? fieldTypeInstance;

        // 处理泛型类型
        if (fieldType.IsGenericType)
        {
            // 获取泛型类型定义和参数
            var genericTypeDefinition = fieldType.GetGenericTypeDefinition();
            var genericArgs = fieldType.GetGenericArguments();

            if (genericTypeDefinition == typeof(OptionsFieldType<>) ||
                genericTypeDefinition == typeof(OptionsMultiFieldType<>))
            {
                // 创建具体的泛型实例
                var concreteType = genericTypeDefinition.MakeGenericType(genericArgs);
                try
                {
                    fieldTypeInstance = (IFieldType)Activator.CreateInstance(concreteType)!;
                    return fieldTypeInstance;
                }
                catch
                {
                    // 创建失败，返回null让外部代码处理
                    return null;
                }
            }
            else
            {
                // 其他未识别的泛型类型
                return null;
            }
        }
        else
        {
            try
            {
                // 对于非泛型类型，直接使用具体类型创建实例
                fieldTypeInstance = (IFieldType)Activator.CreateInstance(fieldType)!;
            }
            catch
            {
                // 如果直接创建失败，尝试从FieldManager获取原型再克隆
                var prototype = _fieldManager.GetFieldType(fieldSystemName);
                if (prototype == null)
                    return null;

                // 简单的深拷贝：创建同类型的新实例
                fieldTypeInstance = (IFieldType)Activator.CreateInstance(prototype.GetType())!;
            }
        }

        // 如果是ArrayFieldType，确保初始化Value属性
        if (fieldTypeInstance is ArrayFieldType arrayFieldType && arrayFieldType.Value == null)
        {
            arrayFieldType.Value = string.Empty;
        }

        return fieldTypeInstance;
    }

    // 获取字段类型系统名称
    private string GetFieldTypeSystemName(Type fieldType)
    {
        // 这里需要根据类型获取对应的系统名称
        // 有多种方式，这里假设类型名称和系统名称有简单映射

        if (fieldType == typeof(TextFieldType))
            return "text";
        else if (fieldType == typeof(BooleanFieldType))
            return "boolean";
        else if (fieldType == typeof(IntegerFieldType))
            return "integer";
        else if (fieldType == typeof(NumberFieldType))
            return "number";
        else if (fieldType == typeof(DateTimeFieldType))
            return "datetime";
        else if (fieldType == typeof(TimeFieldType))
            return "timePicker";
        else if (fieldType == typeof(TextAreaFieldType))
            return "textArea";
        else if (fieldType == typeof(MarkdownFieldType))
            return "markdown";
        else if (fieldType == typeof(UrlFieldType))
            return "url";
        else if (fieldType == typeof(ColorFieldType))
            return "color";
        else if (fieldType == typeof(ImageFieldType))
            return "imageInput";
        else if (fieldType == typeof(FileFieldType))
            return "file";
        else if (fieldType == typeof(ArrayFieldType))
            return "tags";
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(OptionsFieldType<>))
            return "enum";
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(OptionsMultiFieldType<>))
            return "checkboxes";

        return string.Empty;
    }
}

/// <summary>
/// 转换类型定义
/// </summary>
internal class ContentFieldValue
{
    public string FieldName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringConverter))]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 转换器,将不同类型转换为字符串
/// </summary>
internal class JsonStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
        {
            return "true";
        }
        else if (reader.TokenType == JsonTokenType.False)
        {
            return "false";
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out long l))
            {
                return l.ToString();
            }
            else if (reader.TryGetDouble(out double d))
            {
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return string.Empty;
        }
        // 处理数组类型
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var array = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                if (reader.TokenType == JsonTokenType.String)
                {
                    array.Add(reader.GetString() ?? string.Empty);
                }
            }
            return string.Join(",", array);
        }


        return reader.GetString() ?? string.Empty;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}