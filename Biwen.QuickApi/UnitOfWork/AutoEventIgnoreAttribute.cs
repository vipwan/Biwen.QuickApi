// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:11 AutoEventIgnoreAttribute.cs

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// 标注的Entity类型将不会自动广播增删改事件
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AutoEventIgnoreAttribute : Attribute
{
}
