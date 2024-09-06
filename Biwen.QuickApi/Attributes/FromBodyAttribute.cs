// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 FromBodyAttribute.cs

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Biwen.QuickApi
{
    /// <summary>
    /// 标记整个Request对象为FromBody
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class FromBodyAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource? BindingSource => default;
    }
}
