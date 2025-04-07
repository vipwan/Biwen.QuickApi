// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 17:11:56 XRenderSchemaGenerator.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Biwen.QuickApi.Contents.Schema;

/// <summary>
/// XRender Schema生成服务，生成符合FormRender 2.0表单引擎规范的Schema
/// </summary>
public class XRenderSchemaGenerator(IMemoryCache memoryCache) : IContentSchemaGenerator
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private const string CACHE_KEY_PREFIX = "XRenderSchema_";

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

        // XRender Schema 根节点
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

        // 如果不是字段类型，则返回null
        if (!typeof(IFieldType).IsAssignableFrom(propertyType))
            return null;

        // 创建基本schema对象
        var schema = CreateBaseSchema(property);

        // 根据字段类型处理特定配置
        ProcessFieldTypeSpecificConfig(property, schema);

        // 处理通用验证规则
        ProcessCommonValidationRules(property, schema);

        return schema;
    }

    private JsonObject CreateBaseSchema(PropertyInfo property)
    {
        var schema = new JsonObject
        {
            // 设置基本标题
            ["title"] = property.Name,
            ["x-decorator"] = "FormItem"
        };

        // 处理显示名称
        SetDisplayName(property, schema);

        // 处理描述
        var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr is { Description: not null and not "" })
        {
            schema["description"] = descriptionAttr.Description;
        }

        return schema;
    }

    private void SetDisplayName(PropertyInfo property, JsonObject schema)
    {
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>();

        switch (true)
        {
            case bool _ when displayAttr is { Name: not null and not "" }:
                schema["title"] = displayAttr.Name;
                break;

            case bool _ when displayNameAttr is { DisplayName: not null and not "" }:
                schema["title"] = displayNameAttr.DisplayName;
                break;

            case bool _ when property.PropertyType.IsEnum:
                var enumName = property.PropertyType.Name;
                var enumField = property.PropertyType.GetField(enumName);
                if (enumField != null)
                {
                    var descAttr = enumField.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr is { Description: not null and not "" })
                    {
                        schema["title"] = descAttr.Description;
                    }
                }
                break;
        }
    }

    private void ProcessFieldTypeSpecificConfig(PropertyInfo property, JsonObject schema)
    {
        var propertyType = property.PropertyType;

        // 使用模式匹配处理不同字段类型
        switch (propertyType)
        {
            case Type _ when propertyType == typeof(TextFieldType):
                ConfigureTextField(property, schema);
                break;

            case Type _ when propertyType == typeof(UrlFieldType):
                ConfigureUrlField(property, schema);
                break;

            case Type _ when propertyType == typeof(ColorFieldType):
                ConfigureColorField(property, schema);
                break;

            case Type _ when propertyType == typeof(TextAreaFieldType):
                ConfigureTextAreaField(property, schema);
                break;

            case Type _ when propertyType == typeof(MarkdownFieldType):
                ConfigureMarkdownField(property, schema);
                break;

            case Type _ when propertyType == typeof(DateTimeFieldType):
                ConfigureDateTimeField(property, schema);
                break;

            case Type _ when propertyType == typeof(TimeFieldType):
                ConfigureTimeField(property, schema);
                break;

            case Type _ when propertyType == typeof(IntegerFieldType):
                ConfigureIntegerField(property, schema);
                break;

            case Type _ when propertyType == typeof(BooleanFieldType):
                ConfigureBooleanField(property, schema);
                break;

            case Type _ when propertyType == typeof(NumberFieldType):
                ConfigureNumberField(property, schema);
                break;

            case Type _ when propertyType == typeof(ImageFieldType):
                ConfigureImageField(property, schema);
                break;

            case Type _ when propertyType == typeof(FileFieldType):
                ConfigureFileField(property, schema);
                break;

            case Type _ when propertyType == typeof(ArrayFieldType):
                ConfigureArrayField(property, schema);
                break;

            case Type _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(OptionsFieldType<>):
                ConfigureOptionsField(property, schema);
                break;

            case Type _ when propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(OptionsMultiFieldType<>):
                ConfigureOptionsMultiField(property, schema);
                break;
        }
    }

    private void ConfigureTextField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "input";
        schema["x-component"] = "Input";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));

        var props = new JsonObject
        {
            ["placeholder"] = $"请输入{schema["title"]}"
        };

        schema["props"] = props;

        // 处理字符串长度限制
        ProcessStringLengthValidation(property, schema, props);

        // 处理正则表达式验证
        ProcessRegexValidation(property, schema);

        // 处理数据类型验证
        ProcessDataTypeValidation(property, schema, props);
    }

    private void ConfigureUrlField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["format"] = "url";
        schema["widget"] = "input";
        schema["x-component"] = "Input";
        schema["x-validator"] = "url";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));

        schema["props"] = new JsonObject
        {
            ["placeholder"] = "请输入URL"
        };
    }

    private void ConfigureColorField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["format"] = "color";
        schema["widget"] = "color";
        schema["x-component"] = "ColorPicker";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));
    }

    private void ConfigureTextAreaField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "textArea";
        schema["x-component"] = "TextArea";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));

        var props = new JsonObject();
        schema["props"] = props;

        // 处理字符串长度限制
        ProcessStringLengthValidation(property, schema, props);
    }

    private void ConfigureMarkdownField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "markdown";
        schema["x-component"] = "Markdown";

        // 处理Markdown工具栏样式
        var markdownAttr = property.GetCustomAttribute<MarkdownToolBarAttribute>();
        if (markdownAttr != null)
        {
            schema["props"] = new JsonObject
            {
                ["toolStyle"] = markdownAttr.ToolStyle.ToString()
            };
        }

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));
    }

    private void ConfigureDateTimeField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "datePicker";
        schema["x-component"] = "DatePicker";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(DateTime));

        // showTime根据DisplayFormatAttribute来判断,包含HH:mm则为true,否则false
        var showTime = false;
        var displayFormatAttr = property.GetCustomAttribute<DisplayFormatAttribute>();
        if (displayFormatAttr is { DataFormatString: not null and not "" })
        {
            // 判断是否包含HH:mm
            showTime = displayFormatAttr.DataFormatString.Contains("HH:mm");
        }

        schema["props"] = new JsonObject
        {
            ["showTime"] = showTime
        };
    }

    private void ConfigureTimeField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "timePicker";
        schema["x-component"] = "TimePicker";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));

        schema["props"] = new JsonObject
        {
            ["format"] = "HH:mm:ss"
        };
    }

    private void ConfigureIntegerField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "number";
        schema["widget"] = "inputNumber";
        schema["x-component"] = "NumberPicker";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(int));

        var props = new JsonObject
        {
            ["precision"] = 0
        };

        // 处理Range特性
        ProcessRangeValidation(property, schema, props);

        schema["props"] = props;
    }

    private void ConfigureBooleanField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "boolean";
        schema["widget"] = "switch";
        schema["x-component"] = "Switch";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(bool));
    }

    private void ConfigureNumberField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "number";
        schema["widget"] = "inputNumber";
        schema["x-component"] = "NumberPicker";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(double));

        var props = new JsonObject
        {
            ["precision"] = 2
        };

        // 处理Range特性
        if (!ProcessRangeValidation(property, schema, props))
        {
            // 默认范围
            schema["min"] = 0;
            schema["max"] = 100;
        }

        schema["props"] = props;
    }

    private void ConfigureImageField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "imageInput";
        schema["x-component"] = "ImageUploader";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));

        var props = new JsonObject
        {
            ["listType"] = "picture-card"
        };
        schema["props"] = props.DeepClone();
    }

    private void ConfigureFileField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "upload";
        schema["x-component"] = "Upload";

        // 处理默认值
        ProcessDefaultValue(property, schema, typeof(string));

        var props = new JsonObject
        {
            ["listType"] = "text",
            ["multiple"] = false
        };
        schema["props"] = props.DeepClone();
    }

    private void ConfigureArrayField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "array";
        schema["widget"] = "tags";
        schema["x-component"] = "ArrayItems";

        // 根据泛型参数确定数组项的类型
        var itemType = typeof(string); // 默认为字符串类型
        if (property.PropertyType.IsGenericType)
        {
            var genericArgs = property.PropertyType.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                itemType = genericArgs[0];
            }
        }



        // 处理数组长度和数量
        // 存放于props中
        var arrayAttribute = property.GetCustomAttribute<ArrayFieldAttribute>();
        if (arrayAttribute != null)
        {
            var props = new JsonObject
            {
                ["maxCount"] = arrayAttribute.MaxCount,
                ["maxLength"] = arrayAttribute.MaxLength
            };
            schema["props"] = props.DeepClone();
        }

        // 处理默认值
        if (property.PropertyType.IsGenericType)
        {
            // 使用数组类型作为预期类型
            var arrayType = Array.CreateInstance(itemType, 0).GetType();
            ProcessDefaultValue(property, schema, arrayType);
        }
        else
        {
            ProcessDefaultValue(property, schema, typeof(string[]));
        }

        // 配置数组项
        ConfigureArrayItems(property, schema, itemType);
    }

    private void ConfigureArrayItems(PropertyInfo property, JsonObject schema, Type itemType)
    {
        var items = new JsonObject
        {
            ["type"] = GetJsonSchemaType(itemType)
        };

        switch (itemType)
        {
            case Type _ when itemType == typeof(int) || itemType == typeof(long) ||
                           itemType == typeof(double) || itemType == typeof(float) ||
                           itemType == typeof(decimal):
                ConfigureNumberArrayItem(property, items, itemType);
                break;

            case Type _ when itemType == typeof(bool):
                items["widget"] = "switch";
                items["x-component"] = "Switch";
                break;

            case Type _ when itemType == typeof(DateTime):
                items["widget"] = "datePicker";
                items["x-component"] = "DatePicker";
                items["format"] = "date-time";
                var itemProps = new JsonObject
                {
                    ["showTime"] = true
                };
                items["props"] = itemProps.DeepClone();
                break;

            default:
                ConfigureStringArrayItem(property, items);
                break;
        }

        schema["items"] = items;
    }

    private void ConfigureNumberArrayItem(PropertyInfo property, JsonObject items, Type itemType)
    {
        items["widget"] = "inputNumber";
        items["x-component"] = "NumberPicker";

        var props = new JsonObject();

        // 处理Range特性
        var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
        if (rangeAttr != null)
        {
            // 设置最小值和最大值
            items["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
            items["maximum"] = Convert.ToDouble(rangeAttr.Maximum);
            items["min"] = Convert.ToDouble(rangeAttr.Minimum);
            items["max"] = Convert.ToDouble(rangeAttr.Maximum);

            props["min"] = Convert.ToDouble(rangeAttr.Minimum);
            props["max"] = Convert.ToDouble(rangeAttr.Maximum);
        }

        // 根据类型设置精度
        props["precision"] = itemType == typeof(int) || itemType == typeof(long) ? 0 : 2;

        if (props.Count > 0)
        {
            items["props"] = props.DeepClone();
        }
    }

    private void ConfigureStringArrayItem(PropertyInfo property, JsonObject items)
    {
        items["widget"] = "input";
        items["x-component"] = "Input";

        var props = new JsonObject();

        // 处理字符串长度限制
        var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
        if (stringLengthAttr != null)
        {
            if (stringLengthAttr.MinimumLength > 0)
            {
                items["minLength"] = stringLengthAttr.MinimumLength;
                items["min"] = stringLengthAttr.MinimumLength;
            }
            items["maxLength"] = stringLengthAttr.MaximumLength;
            items["max"] = stringLengthAttr.MaximumLength;
            props["maxLength"] = stringLengthAttr.MaximumLength;
        }

        if (props.Count > 0)
        {
            items["props"] = props.DeepClone();
        }
    }

    private void ConfigureOptionsField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "string";
        schema["widget"] = "radio";
        schema["x-component"] = "Radio.Group";

        // 获取枚举类型
        var enumType = property.PropertyType.GetGenericArguments()[0];

        // 处理默认值
        ProcessEnumDefaultValue(property, schema);

        // 创建选项数组
        var options = CreateEnumOptions(enumType);

        schema["props"] = new JsonObject
        {
            ["options"] = options.DeepClone()
        };
    }

    private void ConfigureOptionsMultiField(PropertyInfo property, JsonObject schema)
    {
        schema["type"] = "array";
        schema["widget"] = "checkboxes";
        schema["x-component"] = "Checkbox.Group";

        // 获取枚举类型
        var enumType = property.PropertyType.GetGenericArguments()[0];

        // 处理默认值
        ProcessEnumDefaultValue(property, schema);

        // 创建选项数组
        var options = CreateEnumOptions(enumType);

        schema["props"] = new JsonObject
        {
            ["options"] = options.DeepClone()
        };
    }

    private void ProcessEnumDefaultValue(PropertyInfo property, JsonObject schema)
    {
        var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
        if (defaultValueAttr?.Value != null)
        {
            try
            {
                if (defaultValueAttr.Value.GetType().IsEnum)
                {
                    var enumValue = Convert.ToInt32(defaultValueAttr.Value);
                    schema["default"] = enumValue.ToString();
                    schema["defaultValue"] = enumValue.ToString();
                }
            }
            catch
            {
                // 如果转换失败，忽略默认值
            }
        }
    }

    private JsonArray CreateEnumOptions(Type enumType)
    {
        var options = new JsonArray();

        foreach (var enumValue in Enum.GetValues(enumType))
        {
            var name = enumValue.ToString();
            var description = name;

            // 获取字段信息及其特性
            var fieldInfo = enumType.GetField(name!);
            if (fieldInfo != null)
            {
                description = GetEnumValueDescription(fieldInfo) ?? name;
            }

            options.Add(new JsonObject
            {
                ["label"] = description,
                ["value"] = Convert.ToInt32(enumValue).ToString()
            });
        }

        return options;
    }

    private string? GetEnumValueDescription(FieldInfo fieldInfo)
    {
        // 优先使用Display特性
        var displayAttr = fieldInfo.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr is { Name: not null and not "" })
        {
            return displayAttr.Name;
        }

        // 其次使用DisplayName特性
        var displayNameAttr = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr is { DisplayName: not null and not "" })
        {
            return displayNameAttr.DisplayName;
        }

        // 最后使用Description特性
        var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr is { Description: not null and not "" })
        {
            return descAttr.Description;
        }

        return null;
    }

    private void ProcessCommonValidationRules(PropertyInfo property, JsonObject schema)
    {
        // 处理必填属性
        ProcessRequiredValidation(property, schema);

        // 处理比较验证
        ProcessCompareValidation(property, schema);
    }

    private void ProcessRequiredValidation(PropertyInfo property, JsonObject schema)
    {
        var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
        if (requiredAttr != null)
        {
            schema["required"] = true;

            // 如果有错误消息，则添加到schema中
            if (!string.IsNullOrEmpty(requiredAttr.ErrorMessage))
            {
                if (schema["message"] == null)
                {
                    schema["message"] = new JsonObject();
                }
                ((JsonObject)schema["message"]!)["required"] = requiredAttr.ErrorMessage;
            }
        }
    }

    private void ProcessCompareValidation(PropertyInfo property, JsonObject schema)
    {
        var compareAttr = property.GetCustomAttribute<CompareAttribute>();
        if (compareAttr != null)
        {
            // FormRender 2.0 自定义验证
            schema["x-validator"] = new JsonObject
            {
                ["validator"] = $"(value) => {{ return value === form.values.{compareAttr.OtherProperty} ? '' : '{compareAttr.ErrorMessage ?? $"必须与{compareAttr.OtherProperty}相同"}'; }}",
                ["depends"] = new JsonArray { compareAttr.OtherProperty }
            };
        }
    }

    private void ProcessStringLengthValidation(PropertyInfo property, JsonObject schema, JsonObject props)
    {
        // 处理字符串长度限制
        var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
        if (stringLengthAttr != null)
        {
            if (stringLengthAttr.MinimumLength > 0)
            {
                schema["minLength"] = stringLengthAttr.MinimumLength;
                schema["min"] = stringLengthAttr.MinimumLength;
            }
            schema["maxLength"] = stringLengthAttr.MaximumLength;
            schema["max"] = stringLengthAttr.MaximumLength;
            props["maxLength"] = stringLengthAttr.MaximumLength;
        }

        // 处理最小长度限制
        var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
        if (minLengthAttr != null && stringLengthAttr == null)
        {
            schema["minLength"] = minLengthAttr.Length;
            schema["min"] = minLengthAttr.Length;
        }
    }

    private bool ProcessRangeValidation(PropertyInfo property, JsonObject schema, JsonObject props)
    {
        // 处理Range特性
        var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
        if (rangeAttr != null)
        {
            // 设置最小值和最大值 - FormRender 2.0 格式
            schema["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
            schema["maximum"] = Convert.ToDouble(rangeAttr.Maximum);
            schema["min"] = Convert.ToDouble(rangeAttr.Minimum);
            schema["max"] = Convert.ToDouble(rangeAttr.Maximum);

            // 组件属性中的min和max
            props["min"] = Convert.ToDouble(rangeAttr.Minimum);
            props["max"] = Convert.ToDouble(rangeAttr.Maximum);

            return true;
        }

        return false;
    }

    private void ProcessRegexValidation(PropertyInfo property, JsonObject schema)
    {
        // 处理正则表达式验证
        var regexAttr = property.GetCustomAttribute<RegularExpressionAttribute>();
        if (regexAttr != null)
        {
            schema["pattern"] = regexAttr.Pattern;
            schema["x-validator"] = "pattern";

            // 添加错误消息对象
            if (schema["message"] == null)
            {
                schema["message"] = new JsonObject();
            }
            if (!string.IsNullOrEmpty(regexAttr.ErrorMessage))
            {
                ((JsonObject)schema["message"]!)["pattern"] = regexAttr.ErrorMessage;
            }
            else
            {
                ((JsonObject)schema["message"]!)["pattern"] = "请输入正确的格式";
            }
        }
    }

    private void ProcessDataTypeValidation(PropertyInfo property, JsonObject schema, JsonObject props)
    {
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
                    ((JsonObject)schema["props"]!)["placeholder"] = "请输入邮箱地址";
                    break;
                case DataType.PhoneNumber:
                    schema["format"] = "tel";
                    schema["x-validator"] = "phone";
                    props["placeholder"] = "请输入电话号码";
                    ((JsonObject)schema["props"]!)["placeholder"] = "请输入电话号码";
                    break;
                case DataType.Url:
                    schema["format"] = "url";
                    schema["x-validator"] = "url";
                    props["placeholder"] = "请输入URL";
                    ((JsonObject)schema["props"]!)["placeholder"] = "请输入URL";
                    break;
                case DataType.Password:
                    schema["widget"] = "password";
                    schema["x-component"] = "Password";
                    break;
                case DataType.MultilineText:
                    schema["widget"] = "textarea";
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
            ((JsonObject)schema["props"]!)["placeholder"] = "请输入邮箱地址";
        }

        // 处理电话号码特性
        var phoneAttr = property.GetCustomAttribute<PhoneAttribute>();
        if (phoneAttr != null)
        {
            schema["format"] = "tel";
            schema["x-validator"] = "phone";
            props["placeholder"] = "请输入电话号码";
            ((JsonObject)schema["props"]!)["placeholder"] = "请输入电话号码";

            // 添加错误消息
            if (schema["message"] == null)
            {
                schema["message"] = new JsonObject();
            }
            ((JsonObject)schema["message"]!)["pattern"] = "Invalid phone number format.";
        }
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
                    schema["defaultValue"] = defaultValueAttr.Value.ToString();
                }
                else if (expectedType == typeof(bool) && defaultValueAttr.Value is bool boolValue)
                {
                    schema["default"] = boolValue;
                    schema["defaultValue"] = boolValue;
                }
                else if ((expectedType == typeof(int) || expectedType == typeof(double) ||
                         expectedType == typeof(float) || expectedType == typeof(decimal) ||
                         expectedType == typeof(long)) &&
                         defaultValueAttr.Value is IConvertible)
                {
                    var convertedValue = Convert.ToDouble(defaultValueAttr.Value);
                    schema["default"] = convertedValue;
                    schema["defaultValue"] = convertedValue;
                }
                else if (expectedType == typeof(DateTime) && defaultValueAttr.Value is DateTime dateValue)
                {
                    var dateStr = dateValue.ToString("yyyy-MM-dd");
                    schema["default"] = dateStr;
                    schema["defaultValue"] = dateStr;
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
                    schema["defaultValue"] = arrayNode.DeepClone();
                }
            }
            catch
            {
                // 如果转换失败，忽略默认值
            }
        }
    }

}
