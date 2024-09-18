// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:51 StringExtensions.cs

using Biwen.QuickApi.Infrastructure.Html;

namespace Biwen.QuickApi;

[SuppressType]
public static class StringExtensions
{
    /// <summary>
    /// xss过滤
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    /// <exception cref="QuickApiExcetion">必须启用UseBiwenQuickApis()</exception>
    public static string SanitizeHtml(this string html)
    {
        if (ServiceRegistration.ServiceProvider is null) throw new QuickApiExcetion("mush UseBiwenQuickApis() first!");
        var svc = ActivatorUtilities.GetServiceOrCreateInstance<IHtmlSanitizerService>(ServiceRegistration.ServiceProvider);
        return svc.Sanitize(html);
    }
}