// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:40 IHtmlSanitizerService.cs

namespace Biwen.QuickApi.Infrastructure.Html;

public interface IHtmlSanitizerService
{
    /// <summary>
    /// 消除HTML中的XSS攻击
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    string Sanitize(string html);
}
