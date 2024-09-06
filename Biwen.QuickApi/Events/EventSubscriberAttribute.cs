// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 EventSubscriberAttribute.cs

namespace Biwen.QuickApi.Events;

/// <summary>
/// 事件订阅的Metadata
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class EventSubscriberAttribute : Attribute
{
    /// <summary>
    /// 如果发生错误是否抛出异常,将阻塞后续订阅 默认:false
    /// </summary>
    public bool ThrowIfError { get; set; } = false;

    /// <summary>
    /// 执行排序 默认:0
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// 是否异步执行(非阻塞模式),默认:false
    /// </summary>
    public bool IsAsync { get; set; } = false;

}