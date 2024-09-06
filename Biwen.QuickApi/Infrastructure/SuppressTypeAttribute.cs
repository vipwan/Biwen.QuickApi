// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:02 SuppressTypeAttribute.cs

namespace Biwen.QuickApi.Infrastructure
{
    /// <summary>
    /// 不被自动扫描注入的类型,如果开发者主动注入该特性将无意义
    /// </summary>
    [SuppressType, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SuppressTypeAttribute : Attribute
    {
    }
}