// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:31 HtmlSanitizerOptionsExtensions.cs

using Ganss.Xss;
using HtmlSanitizerOptions = Biwen.QuickApi.Infrastructure.Html.HtmlSanitizerOptions;

namespace Microsoft.Extensions.DependencyInjection
{
    [SuppressType]
    public static class HtmlSanitizerOptionsExtensions
    {
        /// <summary>
        /// Adds a configuration action to the html sanitizer.
        /// </summary>
        public static void ConfigureHtmlSanitizer(this IServiceCollection services, Action<HtmlSanitizer> action)
        {
            services.Configure<HtmlSanitizerOptions>(o =>
            {
                o.Configure.Add(action);
            });
        }
    }
}
