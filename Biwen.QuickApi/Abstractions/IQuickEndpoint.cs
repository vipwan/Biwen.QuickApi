// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IQuickEndpoint.cs

namespace Biwen.QuickApi.Abstractions;

/// <summary>
/// 对Minimal Api的扩展
/// </summary>
public interface IQuickEndpoint
{
    /// <summary>
    /// 请求方式 支持多个,如: Verb.GET | Verb.POST,默认:Verb.GET
    /// </summary>
    /// <returns></returns>
    public static abstract Verb Verbs { get; }
    /// <summary>
    /// MinimalApi执行的Handler
    /// </summary>
    /// <returns></returns>
    public static abstract Delegate Handler { get; }

}
