// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:24 IQuickApiExceptionResultBuilder.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http;

/// <summary>
/// 500异常结果
/// </summary>
public interface IQuickApiExceptionResultBuilder
{
    /// <summary>
    /// 规范化返回异常
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    Task<IResult> ErrorResultAsync(Exception exception);
}