// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:45:46 ServiceRegistration.cs

using Biwen.QuickApi.Infrastructure.Html;

namespace Microsoft.Extensions.DependencyInjection;

[SuppressType]
internal static partial class ServiceRegistration
{
    /// <summary>
    /// Adds html script sanitization services.
    /// </summary>
    internal static IServiceCollection AddHtmlSanitizer(this IServiceCollection services)
    {

        services.AddOptions<HtmlSanitizerOptions>();

        services.ConfigureHtmlSanitizer((sanitizer) =>
        {
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedTags.Remove("form");
        });

        services.AddActivatedSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
        return services;

    }
}
