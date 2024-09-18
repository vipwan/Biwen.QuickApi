// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:27 HtmlSanitizerOptions.cs

using Ganss.Xss;

namespace Biwen.QuickApi.Infrastructure.Html;

public class HtmlSanitizerOptions
{
    public List<Action<HtmlSanitizer>> Configure { get; } = [];
}
