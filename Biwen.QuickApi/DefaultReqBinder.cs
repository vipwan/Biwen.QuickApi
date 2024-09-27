﻿// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
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
using System.Text;
using System.Text.Json;

namespace Biwen.QuickApi;

/// <summary>
/// 默认的内部绑定器
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultReqBinder<T> : IReqBinder<T> where T : BaseRequest<T>, new()
{
    /// <summary>
    /// 可以重写此方法，实现自定义绑定,
    /// 如果需要表单绑定,以及IFormFile绑定，请务必重写此方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static async ValueTask<T> BindAsync(HttpContext context, ParameterInfo parameter = null!)
    {
        IDictionary<string, object>? ReadFromJsonDic = null;

        //route > header > body(Post) = querystring(Get)
        var @default = new T();
        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (props?.Length == 0) return @default;

        if (context.Request.Method != HttpMethods.Get && context.Request.Method != HttpMethods.Head
            && type.GetCustomAttribute<FromBodyAttribute>() is not null)
        {
            @default = await context.Request.ReadFromJsonAsync<T>();
            return @default!;
        }

        foreach (var prop in props!)
        {
            //if (prop.Name == nameof(BaseRequest<T>.RealValidator)) continue;
            //如果属性不可读|写,则跳过
            if (prop.CanWrite == false || prop.CanRead == false) continue;

            #region IFormFile 和 IFormFileCollection 绑定

            if (prop.PropertyType == typeof(IFormFile) && context.Request.Form.Files.SingleOrDefault() is { } file)
            {
                prop.SetValue(@default, file);
                continue;
            }
            if (prop.PropertyType == typeof(IFormFileCollection) && context.Request.Form.Files is { } files)
            {
                prop.SetValue(@default, files);
                continue;
            }

            #endregion

            if (prop.GetCustomAttribute<FromQueryAttribute>() is { } fromQuery)
            {
                var name = fromQuery.Name ?? prop.Name;
                var qs = context.Request.Query;
                if (string.IsNullOrEmpty(qs[name]))
                {
                    continue;
                }
                //数组
                var value = StringValuesExtensions.DeserializeFromStringValues(qs[name], prop.PropertyType);
                //var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name].ToString());
                prop.SetValue(@default, value);
                continue;
            }

            if (prop.GetCustomAttribute<FromHeaderAttribute>() is { } fromHeader)
            {
                var name = fromHeader.Name ?? prop.Name;
                var qs = context.Request.Headers;
                if (qs.TryGetValue(name, out StringValues val))
                {
                    //转换
                    var value = StringValuesExtensions.DeserializeFromStringValues(val, prop.PropertyType);
                    prop.SetValue(@default, value);
                    continue;
                }
            }

            if (prop.GetCustomAttribute<FromRouteAttribute>() is { } fromRoute)
            {
                var name = fromRoute.Name ?? prop.Name;
                var qs = context.Request.RouteValues;
                if (qs.ContainsKey(name))
                {
                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name]?.ToString()!);
                    prop.SetValue(@default, value);
                    continue;
                }
            }

            if (prop.GetCustomAttribute<FromFormAttribute>() is { } fromForm)
            {
                var name = fromForm.Name ?? prop.Name;
                var qs = context.Request.Form;
                if (qs.ContainsKey(name))
                {
                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name].ToString());
                    prop.SetValue(@default, value);
                    continue;
                }
            }

            if (prop.GetCustomAttribute<FromBodyAttribute>() is { } fromBody)
            {
                var value = await context.Request.ReadFromJsonAsync(prop.PropertyType);
                prop.SetValue(@default, value);
                continue;
            }

            if (prop.CustomAttributes.OfType<IBindingSourceMetadata>().Any())
            {
                //如果标记的bind特性,不管是否找到,都不再继续查找(最高权重)
                continue;
            }

            //route & 路由不支持别名
            //route只支持基础类型,不可使用复杂类型 这属于约定!
            {
                var qs = context.Request.RouteValues;
                if (qs.ContainsKey(prop.Name))
                {
                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[prop.Name]?.ToString()!);
                    prop.SetValue(@default, value);
                    continue;
                }
            }
            //如果仍然未找到
            {
                var requestMethod = context.Request.Method!;
                if (requestMethod == HttpMethods.Get)
                {
                    //querystring
                    {
                        var qs = context.Request.Query;
                        var value = StringValuesExtensions.DeserializeFromStringValues(qs[prop.Name], prop.PropertyType);
                        prop.SetValue(@default, value);
                        continue;
                    }
                }
                if (requestMethod == HttpMethods.Head)
                {
                    //header
                    {
                        var qs = context.Request.Headers;
                        var value = qs[prop.Name];
                        if (value.Count > 0)
                        {
                            //转换
                            var value2 = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(value[0]!);
                            prop.SetValue(@default, value2);
                            continue;
                        }
                    }
                }

                if (requestMethod == HttpMethods.Post ||
                    requestMethod == HttpMethods.Put ||
                    requestMethod == HttpMethods.Options ||
                    requestMethod == HttpMethods.Patch ||
                    requestMethod == HttpMethods.Delete)
                {

                    //是否上传文件
                    bool hasFormFile = prop.PropertyType == typeof(IFormFile) || prop.PropertyType == typeof(IFormFileCollection);
                    //body
                    {
                        //如果是上传文件,则跳过
                        if (hasFormFile) continue;

                        //如果是FromHeader:
                        if (prop.GetCustomAttribute<FromHeaderAttribute>() is { })
                        {
                            var qs = context.Request.Headers;
                            var headers = qs[prop.Name];
                            if (headers.Count > 0)
                            {
                                //转换
                                var value2 = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(headers[0]!);
                                prop.SetValue(@default, value2);
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        //如果是Query:
                        if (prop.GetCustomAttribute<FromQueryAttribute>() is { })
                        {
                            var qs = context.Request.Query;
                            var query = StringValuesExtensions.DeserializeFromStringValues(qs[prop.Name], prop.PropertyType);
                            prop.SetValue(@default, query);
                            continue;
                        }
                        //如果是Route:
                        if (prop.GetCustomAttribute<FromRouteAttribute>() is { })
                        {
                            var qs = context.Request.RouteValues;
                            if (qs.ContainsKey(prop.Name))
                            {
                                var route = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[prop.Name]?.ToString()!);
                                prop.SetValue(@default, route);
                                continue;
                            }
                        }

                        try
                        {
                            ReadFromJsonDic ??= (await context.Request.ReadFromJsonAsync<ExpandoObject>())!;
                        }
                        catch
                        {
                            throw new QuickApiExcetion("无法从RequestBody绑定对象!");
                        }

                        //注意别名权重高于属性名
                        var currentKey = (prop.Name).ToLower();

                        //忽略大小写
                        var ignoreCasDic = ReadFromJsonDic!.Select(x => new KeyValuePair<string, object>(x.Key.ToLower(), x.Value))
                            .ToDictionary(x => x.Key, x => x.Value);

                        if (ignoreCasDic.TryGetValue(currentKey, out object? value))
                        {
                            //转换
                            if (value == null)
                            {
                                prop.SetValue(@default, null);
                            }
                            else
                            {
                                var value2 = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(value.ToString()!);
                                prop.SetValue(@default, value2);
                            }
                        }
                    }
                }
            }
        }

        //返回
        return @default ?? new();
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
