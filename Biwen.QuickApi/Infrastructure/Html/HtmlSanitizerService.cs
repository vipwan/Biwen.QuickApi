// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:36 HtmlSanitizerService.cs

using Ganss.Xss;

namespace Biwen.QuickApi.Infrastructure.Html;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer = new();

    public HtmlSanitizerService(IOptions<HtmlSanitizerOptions> options)
    {
        foreach (var action in options.Value.Configure)
        {
            action?.Invoke(_sanitizer);
        }
    }

    public string Sanitize(string html) => _sanitizer.Sanitize(html);
}
