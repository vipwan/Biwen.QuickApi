// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:14 HtmlSanitizerTests.cs

using Biwen.QuickApi.Infrastructure.Html;
using Microsoft.Extensions.Options;

namespace Biwen.QuickApi.Test;

public class HtmlSanitizerTests
{
    private static readonly HtmlSanitizerService _sanitizer = new(Options.Create(new HtmlSanitizerOptions()));

    [Theory]
    [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
    [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
    [InlineData("<a href=\"javascript: alert('xss')\">Click me</a>", @"<a>Click me</a>")]
    [InlineData("<a href=\"[locale 'en']javascript: alert('xss')[/locale]\">Click me</a>", @"<a>Click me</a>")]
    public void ShouldSanitizeHTML(string html, string sanitized)
    {
        // Setup
        var output = _sanitizer.Sanitize(html);

        // Test
        Assert.Equal(output, sanitized);
    }

    [Fact]
    public void ShouldConfigureSanitizer()
    {
        var services = new ServiceCollection();
        services.AddOptions<HtmlSanitizerOptions>();
        services.ConfigureHtmlSanitizer((sanitizer) =>
        {
            sanitizer.AllowedAttributes.Add("class");
        });

        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        var sanitizer = services.BuildServiceProvider().GetService<IHtmlSanitizerService>();

        var input = @"<a href=""bar"" class=""foo"">baz</a>";
        var sanitized = sanitizer!.Sanitize(input);
        Assert.Equal(input, sanitized);
    }

    [Fact]
    public void ShouldReconfigureSanitizer()
    {
        // Setup. With defaults.
        var services = new ServiceCollection();
        services.AddOptions<HtmlSanitizerOptions>();
        services.ConfigureHtmlSanitizer((sanitizer) =>
        {
            sanitizer.AllowedAttributes.Add("class");
        });

        // Act. Reconfigure to remove defaults.
        services.Configure<HtmlSanitizerOptions>(o =>
        {
            o.Configure.Clear();
        });

        // Test.
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        var sanitizer = services.BuildServiceProvider().GetService<IHtmlSanitizerService>();

        var input = @"<a href=""bar"" class=""foo"">baz</a>";
        var sanitized = sanitizer!.Sanitize(input);
        Assert.Equal(@"<a href=""bar"">baz</a>", sanitized);
    }
}
