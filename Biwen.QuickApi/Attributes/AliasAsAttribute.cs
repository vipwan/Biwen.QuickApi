// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 AliasAsAttribute.cs

namespace Biwen.QuickApi.Attributes
{
    /// <summary>
    /// 针对属性的别名,
    /// 一般用于Rsp,如果需要用于Req,请自定义Binder或者修改传输对象属性为Alias
    /// </summary>

    [Obsolete("不再提供别名支持")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class AliasAsAttribute(string name) : Attribute
    {
        public string Name { get; set; } = name;
    }
}