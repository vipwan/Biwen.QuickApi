// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:19 IQuickApiExceptionHandler.cs

namespace Biwen.QuickApi.Http;

/// <summary>
/// 全局QuickApi异常处理
/// </summary>
public interface IQuickApiExceptionHandler
{
    Task HandleAsync(Exception exception);
}