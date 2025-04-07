// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 17:11:56 FormilySchemaGenerator.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Biwen.QuickApi.Contents.Schema;

/// <summary>
/// Formily Schema生成服务，生成符合Formily 2.0表单引擎规范的Schema
/// </summary>
public class FormilySchemaGenerator : IContentSchemaGenerator
{
    private readonly IMemoryCache _memoryCache;
    private const string CACHE_KEY_PREFIX = "FormilySchema_";

    public FormilySchemaGenerator(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public virtual JsonObject GenerateSchema<T>() where T : IContent
    {
        return GenerateSchema(typeof(T));
    }

    public virtual JsonObject GenerateSchema(Type contentType)
    {
        if (!typeof(IContent).IsAssignableFrom(contentType))
        {
            throw new ArgumentException($"类型 {contentType.Name} 必须实现 IContent 接口");
        }

        // 尝试从缓存获取
        string cacheKey = $"{CACHE_KEY_PREFIX}{contentType.FullName}";
        if (_memoryCache.TryGetValue(cacheKey, out JsonObject? cachedSchema) && cachedSchema != null)
        {
            return cachedSchema;
        }

        // Formily Schema 根节点
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
        };

        // 定义需要的字段
        var required = new JsonArray();

        var properties = contentType.GetProperties();
        foreach (var property in properties)
        {
            if (!typeof(IFieldType).IsAssignableFrom(property.PropertyType))
                continue;

            var propertySchema = GeneratePropertySchema(property);
            if (propertySchema != null)
            {
                ((JsonObject)schema["properties"]!)[property.Name] = propertySchema;

                // 如果是必填字段，添加到required数组中
                var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttr != null)
                {
                    required.Add(property.Name);
                }
            }
        }

        // 如果有必填字段，添加到schema中
        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        // 将结果存入缓存
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(24)) // 设置滑动过期时间为24小时
            .SetAbsoluteExpiration(TimeSpan.FromDays(7)); // 设置绝对过期时间为7天

        _memoryCache.Set(cacheKey, schema, cacheOptions);

        return schema;
    }

    public virtual string GenerateSchemaJson<T>(JsonSerializerOptions? options = null) where T : IContent
    {
        return GenerateSchemaJson(typeof(T), options);
    }

    public virtual string GenerateSchemaJson(Type contentType, JsonSerializerOptions? options = null)
    {
        var schema = GenerateSchema(contentType);
        return JsonSerializer.Serialize(schema, options ?? new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private JsonObject? GeneratePropertySchema(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var schema = new JsonObject
        {
            ["title"] = property.Name,
            ["x-decorator"] = "FormItem"
        };

        // 获取显示名称特性
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
        {
            schema["title"] = displayAttr.Name;
        }

        // 获取描述特性
        var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr != null && !string.IsNullOrEmpty(descriptionAttr.Description))
        {
            schema["description"] = descriptionAttr.Description;
        }

        // 处理文本字段类型
        if (propertyType == typeof(TextFieldType))
        {
            schema["type"] = "string";
            schema["x-component"] = "Input";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            // 处理组件属性
            var props = new JsonObject();

            // 处理字符串长度限制
            var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttr != null)
            {
                if (stringLengthAttr.MinimumLength > 0)
                {
                    schema["minLength"] = stringLengthAttr.MinimumLength;
                }
                schema["maxLength"] = stringLengthAttr.MaximumLength;
                props["maxLength"] = stringLengthAttr.MaximumLength;
            }

            // 处理最小长度限制
            var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null && stringLengthAttr == null)
            {
                schema["minLength"] = minLengthAttr.Length;
            }

            // 处理正则表达式验证
            var regexAttr = property.GetCustomAttribute<RegularExpressionAttribute>();
            if (regexAttr != null)
            {
                schema["pattern"] = regexAttr.Pattern;
                schema["x-validator"] = "pattern";
            }

            // 处理数据类型验证
            var dataTypeAttr = property.GetCustomAttribute<DataTypeAttribute>();
            if (dataTypeAttr != null)
            {
                switch (dataTypeAttr.DataType)
                {
                    case DataType.EmailAddress:
                        schema["format"] = "email";
                        schema["x-validator"] = "email";
                        props["placeholder"] = "请输入邮箱地址";
                        break;
                    case DataType.PhoneNumber:
                        schema["format"] = "tel";
                        schema["x-validator"] = "phone";
                        props["placeholder"] = "请输入电话号码";
                        break;
                    case DataType.Url:
                        schema["format"] = "uri";
                        schema["x-validator"] = "url";
                        props["placeholder"] = "请输入URL";
                        break;
                    case DataType.Password:
                        props["type"] = "password";
                        break;
                    case DataType.MultilineText:
                        schema["x-component"] = "TextArea";
                        break;
                }
            }

            // 处理电子邮件特性
            var emailAttr = property.GetCustomAttribute<EmailAddressAttribute>();
            if (emailAttr != null)
            {
                schema["format"] = "email";
                schema["x-validator"] = "email";
                props["placeholder"] = "请输入邮箱地址";
            }

            // 处理电话号码特性
            var phoneAttr = property.GetCustomAttribute<PhoneAttribute>();
            if (phoneAttr != null)
            {
                schema["format"] = "tel";
                schema["x-validator"] = "phone";
                props["placeholder"] = "请输入电话号码";
            }

            // 添加组件属性
            if (props.Count > 0)
            {
                schema["x-component-props"] = props;
            }
        }
        // 处理URL字段类型
        else if (propertyType == typeof(UrlFieldType))
        {
            schema["type"] = "string";
            schema["format"] = "uri";
            schema["x-component"] = "Input";
            schema["x-validator"] = "url";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject
            {
                ["placeholder"] = "请输入URL"
            };
            schema["x-component-props"] = props;
        }
        // 处理颜色字段类型
        else if (propertyType == typeof(ColorFieldType))
        {
            schema["type"] = "string";
            schema["format"] = "color";
            schema["x-component"] = "ColorPicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));


        }
        // 处理文本区域字段类型
        else if (propertyType == typeof(TextAreaFieldType))
        {
            schema["type"] = "string";
            schema["x-component"] = "TextArea";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));


            var props = new JsonObject();

            // 处理字符串长度限制
            var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttr != null)
            {
                if (stringLengthAttr.MinimumLength > 0)
                {
                    schema["minLength"] = stringLengthAttr.MinimumLength;
                }
                schema["maxLength"] = stringLengthAttr.MaximumLength;
                props["maxLength"] = stringLengthAttr.MaximumLength;
            }

            // 处理最小长度限制
            var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null && stringLengthAttr == null)
            {
                schema["minLength"] = minLengthAttr.Length;
            }

            // 添加组件属性
            if (props.Count > 0)
            {
                schema["x-component-props"] = props;
            }
        }
        // 处理Markdown字段类型
        else if (propertyType == typeof(MarkdownFieldType))
        {
            schema["type"] = "string";
            schema["x-component"] = "Markdown";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

        }
        // 处理日期时间字段类型
        else if (propertyType == typeof(DateTimeFieldType))
        {
            schema["type"] = "string";
            schema["format"] = "date-time";
            schema["x-component"] = "DatePicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(DateTime));


            var props = new JsonObject
            {
                ["showTime"] = true
            };

            // 处理日期格式
            var displayFormatAttr = property.GetCustomAttribute<DisplayFormatAttribute>();
            if (displayFormatAttr != null && !string.IsNullOrEmpty(displayFormatAttr.DataFormatString))
            {
                props["format"] = displayFormatAttr.DataFormatString;
            }

            schema["x-component-props"] = props;
        }
        // 处理整数字段类型
        else if (propertyType == typeof(IntegerFieldType))
        {
            schema["type"] = "number";
            schema["x-component"] = "NumberPicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(int));

            var props = new JsonObject
            {
                ["precision"] = 0
            };

            // 处理Range特性
            var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                // 设置最小值和最大值
                schema["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
                schema["maximum"] = Convert.ToDouble(rangeAttr.Maximum);

                // 同时在组件属性中设置min和max
                props["min"] = Convert.ToDouble(rangeAttr.Minimum);
                props["max"] = Convert.ToDouble(rangeAttr.Maximum);
            }

            schema["x-component-props"] = props;
        }
        // 处理布尔字段类型
        else if (propertyType == typeof(BooleanFieldType))
        {
            schema["type"] = "boolean";
            schema["x-component"] = "Switch";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(bool));
        }
        // 处理数字(浮点数)字段类型
        else if (propertyType == typeof(NumberFieldType))
        {
            schema["type"] = "number";
            schema["x-component"] = "NumberPicker";

            ProcessDefaultValue(property, schema, typeof(double));

            var props = new JsonObject();

            // 处理Range特性
            var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                // 设置最小值和最大值
                schema["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
                schema["maximum"] = Convert.ToDouble(rangeAttr.Maximum);

                // 同时在组件属性中设置min和max
                props["min"] = Convert.ToDouble(rangeAttr.Minimum);
                props["max"] = Convert.ToDouble(rangeAttr.Maximum);
            }

            // 处理精度
            // 对于浮点数，可以设置一个合理的精度，比如2位小数
            props["precision"] = 2;

            // 添加组件属性
            if (props.Count > 0)
            {
                schema["x-component-props"] = props;
            }
        }
        // 处理图片字段类型
        else if (propertyType == typeof(ImageFieldType))
        {
            schema["type"] = "string";
            schema["x-component"] = "ImageUploader";

            ProcessDefaultValue(property, schema, typeof(string));


            var props = new JsonObject
            {
                ["listType"] = "picture-card",
                ["accept"] = "image/*"
            };
            schema["x-component-props"] = props;
        }
        // 处理文件字段类型
        else if (propertyType == typeof(FileFieldType))
        {
            schema["type"] = "string";
            schema["x-component"] = "Upload";

            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject
            {
                ["listType"] = "text",
                ["multiple"] = false
            };
            schema["x-component-props"] = props;
        }
        // 处理数组字段类型
        else if (propertyType == typeof(ArrayFieldType))
        {
            schema["type"] = "array";
            schema["x-component"] = "ArrayItems";

            // 根据泛型参数确定数组项的类型
            var itemType = typeof(string); // 默认为字符串类型
            if (propertyType.IsGenericType)
            {
                var genericArgs = propertyType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    itemType = genericArgs[0];
                }
            }

            // 处理默认值
            if (propertyType.IsGenericType)
            {
                // 使用数组类型作为预期类型
                var arrayType = Array.CreateInstance(itemType, 0).GetType();
                ProcessDefaultValue(property, schema, arrayType);
            }
            else
            {
                ProcessDefaultValue(property, schema, typeof(string[]));
            }

            // 根据数组项的类型设置组件
            var items = new JsonObject
            {
                ["type"] = GetJsonSchemaType(itemType)
            };

            // 根据数组项类型选择合适的组件
            if (itemType == typeof(int) || itemType == typeof(long) ||
                itemType == typeof(double) || itemType == typeof(float) ||
                itemType == typeof(decimal))
            {
                items["x-component"] = "NumberPicker";

                // 处理Range特性
                var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
                if (rangeAttr != null)
                {
                    items["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
                    items["maximum"] = Convert.ToDouble(rangeAttr.Maximum);

                    var props = new JsonObject
                    {
                        ["min"] = Convert.ToDouble(rangeAttr.Minimum),
                        ["max"] = Convert.ToDouble(rangeAttr.Maximum)
                    };

                    // 对于整数类型设置precision为0
                    if (itemType == typeof(int) || itemType == typeof(long))
                    {
                        props["precision"] = 0;
                    }

                    items["x-component-props"] = props;
                }
            }
            else if (itemType == typeof(bool))
            {
                items["x-component"] = "Switch";
            }
            else if (itemType == typeof(DateTime))
            {
                items["x-component"] = "DatePicker";
                items["x-component-props"] = new JsonObject
                {
                    ["showTime"] = true
                };
            }
            else
            {
                items["x-component"] = "Input";

                // 处理字符串类型的额外验证
                var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttr != null)
                {
                    if (stringLengthAttr.MinimumLength > 0)
                    {
                        items["minLength"] = stringLengthAttr.MinimumLength;
                    }
                    items["maxLength"] = stringLengthAttr.MaximumLength;
                    items["x-component-props"] = new JsonObject
                    {
                        ["maxLength"] = stringLengthAttr.MaximumLength
                    };
                }
            }

            schema["items"] = items;
        }


        // 在 GeneratePropertySchema 方法中添加处理 OptionsFieldType 和 OptionsMultiFieldType 的代码
        // 处理枚举单选字段类型
        else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(OptionsFieldType<>))
        {
            schema["type"] = "string";
            schema["x-component"] = "Radio.Group";

            // 获取枚举类型
            var enumType = propertyType.GetGenericArguments()[0];

            // 处理默认值
            ProcessDefaultValue(property, schema, enumType);

            // 创建选项数组
            var options = new JsonArray();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                var name = enumValue.ToString();
                var description = name;

                // 获取描述特性
                var fieldInfo = enumType.GetField(name!);
                if (fieldInfo != null)
                {
                    var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                    {
                        description = descAttr.Description;
                    }
                }

                options.Add(new JsonObject
                {
                    ["label"] = description,
                    ["value"] = Convert.ToInt32(enumValue).ToString()
                });
            }

            schema["x-component-props"] = new JsonObject
            {
                ["options"] = options
            };
        }

        else
        {
            return null;
        }

        // 处理必填属性
        var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
        if (requiredAttr != null)
        {
            schema["required"] = true;

            // 如果有错误消息，则添加到schema中
            if (!string.IsNullOrEmpty(requiredAttr.ErrorMessage))
            {
                schema["x-validator"] = new JsonObject
                {
                    ["required"] = true,
                    ["message"] = requiredAttr.ErrorMessage
                };
            }
        }

        // 处理比较验证（例如确认密码）
        var compareAttr = property.GetCustomAttribute<CompareAttribute>();
        if (compareAttr != null)
        {
            schema["x-reactions"] = new JsonObject
            {
                ["dependencies"] = new JsonArray { compareAttr.OtherProperty },
                ["fulfill"] = new JsonObject
                {
                    ["state"] = new JsonObject
                    {
                        ["selfErrors"] = new JsonObject
                        {
                            ["$deps"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["type"] = "formula",
                                ["formula"] = $"$values.{compareAttr.OtherProperty} !== $value ? '{compareAttr.ErrorMessage ?? $"必须与{compareAttr.OtherProperty}相同"}' : ''"
                            }
                        }
                        }
                    }
                }
            };
        }

        return schema;
    }

    // 辅助方法：根据.NET类型获取JSON Schema的类型
    private string GetJsonSchemaType(Type type)
    {
        if (type == typeof(string))
            return "string";
        else if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            return "number";
        else if (type == typeof(bool))
            return "boolean";
        else if (type == typeof(DateTime))
            return "string"; // 日期在JSON Schema中通常表示为带有format的字符串
        else if (type.IsArray || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            return "array";
        else
            return "object";
    }

    // 通用方法：处理DefaultValue特性
    private void ProcessDefaultValue(PropertyInfo property, JsonObject schema, Type expectedType)
    {
        var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
        if (defaultValueAttr != null && defaultValueAttr.Value != null)
        {
            try
            {
                // 根据不同类型处理默认值
                if (expectedType == typeof(string))
                {
                    schema["default"] = defaultValueAttr.Value.ToString();
                }
                else if (expectedType == typeof(bool) && defaultValueAttr.Value is bool boolValue)
                {
                    schema["default"] = boolValue;
                }
                else if ((expectedType == typeof(int) || expectedType == typeof(double) ||
                         expectedType == typeof(float) || expectedType == typeof(decimal) ||
                         expectedType == typeof(long)) &&
                         defaultValueAttr.Value is IConvertible)
                {
                    var convertedValue = Convert.ToDouble(defaultValueAttr.Value);
                    schema["default"] = convertedValue;
                }
                else if (expectedType == typeof(DateTime) && defaultValueAttr.Value is DateTime dateValue)
                {
                    schema["default"] = dateValue.ToString("yyyy-MM-ddTHH:mm:ss");
                }
                else if (defaultValueAttr.Value.GetType().IsArray && expectedType.IsArray)
                {
                    // 处理数组类型的默认值
                    JsonNode arrayNode = new JsonArray();
                    foreach (var item in (Array)defaultValueAttr.Value)
                    {
                        ((JsonArray)arrayNode).Add(JsonValue.Create(item));
                    }
                    schema["default"] = arrayNode;
                }
            }
            catch
            {
                // 如果转换失败，忽略默认值
            }
        }
    }


}