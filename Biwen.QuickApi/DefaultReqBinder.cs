// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:09:55 DefaultReqBinder.cs

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Biwen.QuickApi;

/// <summary>
/// 默认的内部绑定器
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultReqBinder<T> : IReqBinder<T> where T : BaseRequest<T>, new()
{
    // 缓存类型的属性信息
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    // 缓存类型的FromBody特性
    private static readonly ConcurrentDictionary<Type, bool> _fromBodyAttributeCache = new();

    // 缓存属性设置器
    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> _setterCache = new();

    // JSON序列化选项
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 可以重写此方法，实现自定义绑定,
    /// 如果需要表单绑定,以及IFormFile绑定，请务必重写此方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static async ValueTask<T> BindAsync(HttpContext context, ParameterInfo parameter = null!)
    {
        //route > header > body(Post) = querystring(Get)
        var @default = new T();
        var type = typeof(T);

        // 从缓存获取类型属性
        var props = _propertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (props.Length == 0) return @default;

        // 检查类型是否有FromBody特性
        var hasFromBodyAttribute = _fromBodyAttributeCache.GetOrAdd(type, t =>
            t.GetCustomAttribute<FromBodyAttribute>() != null);

        // 如果请求方法不是GET或HEAD，且类型标记了FromBody，直接绑定整个对象
        if (context.Request.Method != HttpMethods.Get &&
            context.Request.Method != HttpMethods.Head &&
            hasFromBodyAttribute)
        {
            @default = await context.Request.ReadFromJsonAsync<T>(_jsonOptions);
            return @default!;
        }

        // 延迟初始化RequestBody字典
        IDictionary<string, object>? bodyDictionary = null;
        Dictionary<string, object>? ignoreCaseDictionary = null;

        // 标记是否需要处理表单
        bool formProcessed = false;
        IFormCollection? formCollection = null;

        foreach (var prop in props)
        {
            // 跳过不可读写的属性
            if (!prop.CanWrite || !prop.CanRead) continue;

            // 获取属性设置器
            var setter = _setterCache.GetOrAdd(prop, CreatePropertySetter);

            // 处理文件上传
            if (prop.PropertyType == typeof(IFormFile) || prop.PropertyType == typeof(IFormFileCollection))
            {
                if (!formProcessed)
                {
                    // 只读取表单一次
                    if (context.Request.HasFormContentType)
                    {
                        formCollection = await context.Request.ReadFormAsync();
                    }
                    formProcessed = true;
                }

                if (formCollection != null)
                {
                    if (prop.PropertyType == typeof(IFormFile) && formCollection.Files.Count > 0)
                    {
                        var file = formCollection.Files[0];
                        setter(@default, file);
                        continue;
                    }
                    else if (prop.PropertyType == typeof(IFormFileCollection) && formCollection.Files.Count > 0)
                    {
                        setter(@default, formCollection.Files);
                        continue;
                    }
                }
            }

            // 处理特性绑定
            if (TryBindFromAttribute(prop, context, @default, setter))
            {
                continue;
            }

            // 如果属性有任何绑定源元数据但没有成功绑定，跳过默认绑定
            if (prop.CustomAttributes.OfType<IBindingSourceMetadata>().Any())
            {
                continue;
            }

            // 尝试从路由绑定（不支持别名）
            if (TryBindFromRoute(prop.Name, prop.PropertyType, context, @default, setter))
            {
                continue;
            }

            // 如果路由未找到，根据HTTP方法尝试其他绑定源
            var requestMethod = context.Request.Method!;

            if (requestMethod == HttpMethods.Get)
            {
                // 从查询字符串绑定
                var qs = context.Request.Query;
                var value = StringValuesExtensions.DeserializeFromStringValues(qs[prop.Name], prop.PropertyType);
                if (value != null)
                {
                    setter(@default, value);
                    continue;
                }
            }
            else if (requestMethod == HttpMethods.Head)
            {
                // 从Header绑定
                var headers = context.Request.Headers;
                if (headers.TryGetValue(prop.Name, out var value) && value.Count > 0)
                {
                    var convertedValue = TypeDescriptor.GetConverter(prop.PropertyType)
                        .ConvertFromInvariantString(value[0]!);
                    setter(@default, convertedValue);
                    continue;
                }
            }
            else if (IsWriteMethod(requestMethod))
            {
                // 处理POST/PUT/PATCH/DELETE等方法

                // 跳过文件类型
                bool isFileType = prop.PropertyType == typeof(IFormFile) ||
                                 prop.PropertyType == typeof(IFormFileCollection);
                if (isFileType) continue;

                // 再次检查特性绑定（可能是POST场景下的FromHeader等）
                if (TryBindFromAttributeInWriteMethod(prop, context, @default, setter))
                {
                    continue;
                }

                // 从请求体读取数据
                try
                {
                    if (bodyDictionary == null)
                    {
                        bodyDictionary = (await context.Request.ReadFromJsonAsync<ExpandoObject>(_jsonOptions))!;

                        if (bodyDictionary != null)
                        {
                            ignoreCaseDictionary = new Dictionary<string, object>(
                                StringComparer.OrdinalIgnoreCase);

                            foreach (var pair in bodyDictionary)
                            {
                                ignoreCaseDictionary[pair.Key] = pair.Value;
                            }
                        }
                    }

                    if (ignoreCaseDictionary != null &&
                        ignoreCaseDictionary.TryGetValue(prop.Name, out object? bodyValue))
                    {
                        if (bodyValue == null)
                        {
                            setter(@default, null);
                        }
                        else
                        {
                            // 尝试转换值
                            var convertedValue = ConvertValue(bodyValue.ToString()!, prop.PropertyType);
                            setter(@default, convertedValue);
                        }
                    }
                }
                catch
                {
                    throw new QuickApiExcetion("无法从RequestBody绑定对象!");
                }
            }
        }

        return @default;
    }

    // 创建属性设置器
    private static Action<object, object?> CreatePropertySetter(PropertyInfo prop)
    {
        var targetParameter = Expression.Parameter(typeof(object), "target");
        var valueParameter = Expression.Parameter(typeof(object), "value");

        var targetCast = Expression.Convert(targetParameter, prop.DeclaringType!);
        var valueCast = Expression.Convert(valueParameter, prop.PropertyType);

        var setterCall = Expression.Call(targetCast, prop.SetMethod!, valueCast);

        return Expression.Lambda<Action<object, object?>>(
            setterCall, targetParameter, valueParameter).Compile();
    }

    // 判断是否为写入方法（POST/PUT等）
    private static bool IsWriteMethod(string method)
    {
        return method == HttpMethods.Post ||
               method == HttpMethods.Put ||
               method == HttpMethods.Patch ||
               method == HttpMethods.Delete ||
               method == HttpMethods.Options;
    }

    // 尝试从特性绑定
    private static bool TryBindFromAttribute(PropertyInfo prop, HttpContext context, T target, Action<object, object?> setter)
    {
        // 处理FromQuery特性
        if (prop.GetCustomAttribute<FromQueryAttribute>() is { } fromQuery)
        {
            var name = fromQuery.Name ?? prop.Name;
            var qs = context.Request.Query;
            if (!string.IsNullOrEmpty(qs[name]))
            {
                var value = StringValuesExtensions.DeserializeFromStringValues(qs[name], prop.PropertyType);
                setter(target, value);
                return true;
            }
        }

        // 处理FromHeader特性
        else if (prop.GetCustomAttribute<FromHeaderAttribute>() is { } fromHeader)
        {
            var name = fromHeader.Name ?? prop.Name;
            if (context.Request.Headers.TryGetValue(name, out StringValues val))
            {
                var value = StringValuesExtensions.DeserializeFromStringValues(val, prop.PropertyType);
                setter(target, value);
                return true;
            }
        }

        // 处理FromRoute特性
        else if (prop.GetCustomAttribute<FromRouteAttribute>() is { } fromRoute)
        {
            var name = fromRoute.Name ?? prop.Name;
            var routeValues = context.Request.RouteValues;
            if (routeValues.TryGetValue(name, out var routeValue))
            {
                var value = TypeDescriptor.GetConverter(prop.PropertyType)
                    .ConvertFromInvariantString(routeValue?.ToString()!);
                setter(target, value);
                return true;
            }
        }

        // 处理FromForm特性
        else if (prop.GetCustomAttribute<FromFormAttribute>() is { } fromForm)
        {
            if (context.Request.HasFormContentType)
            {
                var name = fromForm.Name ?? prop.Name;
                var form = context.Request.Form;
                if (form.ContainsKey(name))
                {
                    var value = TypeDescriptor.GetConverter(prop.PropertyType)
                        .ConvertFromInvariantString(form[name].ToString());
                    setter(target, value);
                    return true;
                }
            }
        }

        // 处理FromBody特性
        else if (prop.GetCustomAttribute<FromBodyAttribute>() is not null)
        {
            var value = context.Request.ReadFromJsonAsync(prop.PropertyType, _jsonOptions).Result;
            setter(target, value);
            return true;
        }

        return false;
    }

    // 在写入方法中再次尝试从特性绑定（处理POST场景下的FromHeader等）
    private static bool TryBindFromAttributeInWriteMethod(PropertyInfo prop, HttpContext context, T target, Action<object, object?> setter)
    {
        // 从Header绑定
        if (prop.GetCustomAttribute<FromHeaderAttribute>() is { })
        {
            var headers = context.Request.Headers;
            var headerValues = headers[prop.Name];
            if (headerValues.Count > 0)
            {
                var value = TypeDescriptor.GetConverter(prop.PropertyType)
                    .ConvertFromInvariantString(headerValues[0]!);
                setter(target, value);
                return true;
            }
            return true; // 即使没找到也返回true，表示已处理
        }

        // 从Query绑定
        if (prop.GetCustomAttribute<FromQueryAttribute>() is { })
        {
            var qs = context.Request.Query;
            var query = StringValuesExtensions.DeserializeFromStringValues(qs[prop.Name], prop.PropertyType);
            setter(target, query);
            return true;
        }

        // 从Route绑定
        if (prop.GetCustomAttribute<FromRouteAttribute>() is { })
        {
            return TryBindFromRoute(prop.Name, prop.PropertyType, context, target, setter);
        }

        return false;
    }

    // 尝试从路由绑定
    private static bool TryBindFromRoute(string name, Type propType, HttpContext context, T target, Action<object, object?> setter)
    {
        var routeValues = context.Request.RouteValues;
        if (routeValues.TryGetValue(name, out var routeValue))
        {
            var value = TypeDescriptor.GetConverter(propType)
                .ConvertFromInvariantString(routeValue?.ToString()!);
            setter(target, value);
            return true;
        }
        return false;
    }

    // 转换值类型
    private static object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (targetType == typeof(string))
            return value;

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value);

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromInvariantString(value);

        // 尝试JSON反序列化
        try
        {
            return JsonSerializer.Deserialize(value, targetType, _jsonOptions);
        }
        catch
        {
            // 如果是值类型，返回默认值
            if (targetType.IsValueType)
                return Activator.CreateInstance(targetType);

            return null;
        }
    }
}

/// <summary>
/// 空绑定器
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class EmptyReqBinder<T> : IReqBinder<T> where T : class, new()
{
    public static ValueTask<T> BindAsync(HttpContext context, ParameterInfo parameter = null!)
    {
        return ValueTask.FromResult(new T());
    }
}


#region BinderExtensions

internal struct ParseResult
{
    public bool IsSuccess { get; set; }

    public object? Value { get; set; }

    public ParseResult(bool isSuccess, object? value)
    {
        IsSuccess = isSuccess;
        Value = value;
    }
}

/// <summary>
/// StringValuesBinderExtensions
/// </summary>
static class StringValuesExtensions
{
    /// <summary>
    /// 默认忽略大小写
    /// </summary>
    static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 将QueryString转换为数组
    /// </summary>
    /// <param name="input"></param>
    /// <param name="tProp"></param>
    /// <returns></returns>
    public static object? DeserializeFromStringValues(StringValues? input, Type tProp)
    {
        if (input is not StringValues vals || vals.Count == 0)
            return null;

        if (tProp.IsEnum)
        {
            return Enum.Parse(tProp, vals[0]!);
        }
        if (tProp == typeof(string))
        {
            return vals[0];
        }
        if (tProp.IsValueType)
        {
            var converter = TypeDescriptor.GetConverter(tProp);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromInvariantString(vals[0]!);
            }
            else
            {
                //值类型如果没有值返回默认值:
                return Activator.CreateInstance(tProp);
            }
        }

        if (vals.Count == 1 && vals[0]!.StartsWith('[') && vals[0]!.EndsWith(']'))
        {
            // querystring: ?ids=[1,2,3]
            // possible inputs:
            // - [1,2,3] (as StringValues[0])
            // - ["one","two","three"] (as StringValues[0])
            // - [{"name":"x"},{"name":"y"}] (as StringValues[0])

            return JsonSerializer.Deserialize(vals[0]!, tProp, _serializerOptions);
        }
        //如果是单个对象且不是字符串,则直接返回
        //?user={"id":"123","name":"x"}
        if (tProp != typeof(string) && !tProp.GetInterfaces().Any(x => x == typeof(IEnumerable)))
        {
            return JsonSerializer.Deserialize(vals[0]!, tProp, _serializerOptions);
        }
        // querystring: ?ids=one&ids=two
        // possible inputs:
        // - 1 (as StringValues)
        // - 1,2,3 (as StringValues)
        // - one (as StringValues)
        // - one,two,three (as StringValues)
        // - [1,2], 2, 3 (as StringValues)
        // - ["one","two"], three, four (as StringValues)
        // - {"name":"x"}, {"name":"y"} (as StringValues) - from swagger ui

        var isEnumArray = false;
        if (tProp.IsArray)
            isEnumArray = tProp.GetElementType()!.IsEnum;

        var sb = new StringBuilder("[");

        for (var i = 0; i < vals.Count; i++)
        {
            if (isEnumArray || (vals[i]!.StartsWith('{') && vals[i]!.EndsWith('}')))
                sb.Append(vals[i]);
            else
            {
                sb.Append('"')
                  .Append(
                       vals[i]!.Contains('"') //json strings with quotations must be escaped
                           ? vals[i]!.Replace("\"", "\\\"")
                           : vals[i])
                  .Append('"');
            }

            if (i < vals.Count - 1)
                sb.Append(',');
        }
        sb.Append(']');

        return JsonSerializer.Deserialize(sb.ToString(), tProp, _serializerOptions);
    }
}


#endregion
