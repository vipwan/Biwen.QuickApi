// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:49:29 ScalarOptions.cs

using System.Text.Json.Serialization;
namespace Biwen.QuickApi.OpenApi.Scalar;
public class ScalarOptions
{
    [JsonIgnore]
    public string EndpointPathPrefix { get; set; } = "/scalar";

    [JsonIgnore]
    public string OpenApiPathPrefix { get; set; } = "/openapi";

    [JsonIgnore]
    public string? Title { get; set; } = "Biwen.QuickApi";

    /// <summary>
    /// default,alternate,moon,purple,solarized
    /// </summary>
    public string Theme { get; set; } = "purple";

    public bool? DarkMode { get; set; } = true;
    public bool? HideDownloadButton { get; set; }
    public bool? ShowSideBar { get; set; }

    public bool? WithDefaultFonts { get; set; }

    public string? Layout { get; set; }

    public string? CustomCss { get; set; }

    public string? SearchHotkey { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }

    public ScalarAuthenticationOptions? Authentication { get; set; }
}

public class ScalarAuthenticationOptions
{
    public string? PreferredSecurityScheme { get; set; }

    public ScalarAuthenticationApiKey? ApiKey { get; set; }
}

public class ScalarAuthenticationoAuth2
{
    public string? ClientId { get; set; }

    public List<string>? Scopes { get; set; }
}

public class ScalarAuthenticationApiKey
{
    public string? Token { get; set; }
}