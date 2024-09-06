// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 FromKeyedServicesAttribute.cs

namespace Biwen.QuickApi
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// 标记绑定属性为FromKeyedServices
    /// </summary>
    [Obsolete("不再提供,如需要注入服务,请直接在构造器中注入即可!")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromKeyedServicesAttribute : Attribute
    {
        public FromKeyedServicesAttribute(string key)
        {
            Key = key;
        }
        public string Key { get; set; }
    }

#endif
}
