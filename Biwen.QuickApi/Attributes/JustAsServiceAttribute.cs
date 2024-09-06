// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 JustAsServiceAttribute.cs

namespace Biwen.QuickApi.Attributes
{
    /// <summary>
    /// 标记QuickApi只在服务中使用,不注册Api路由
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class JustAsServiceAttribute : Attribute
    {
    }
}