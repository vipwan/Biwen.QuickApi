// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:49:24 IEndpointRouteBuilderExtensions.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Biwen.QuickApi.OpenApi.Scalar
{
    [SuppressType]
    public static class IEndpointRouteBuilderExtensions
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// ScalarUi
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapScalarUi(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapScalarUi(static _ => { });
        }

        /// <summary>
        /// ScalarUi
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapScalarUi(this IEndpointRouteBuilder endpoints,
            Action<ScalarOptions> configureOptions)
        {
            var options = new ScalarOptions();
            configureOptions(options);

            var configurationJson = JsonSerializer.Serialize(options, JsonSerializerOptions);

            return endpoints.MapGet(options.EndpointPathPrefix + "/{documentName}", (string documentName) =>
            {
                var title = options.Title ?? $"Scalar API Reference -- {documentName}";
                var openapiPathPrefix = options.OpenApiPathPrefix ?? "/openapi";
                return Results.Content(
                    $$"""
                          <!doctype html>
                          <html>
                          <head>
                              <title>{{title}}</title>
                              <meta charset="utf-8" />
                              <meta name="viewport" content="width=device-width, initial-scale=1" />
                          </head>
                          <body>
                              <script id="api-reference" data-url="{{openapiPathPrefix}}/{{documentName}}.json"></script>
                              <script>
                              var configuration = {
                                  {{configurationJson}}
                              }
                              document.getElementById('api-reference').dataset.configuration =
                                  JSON.stringify(configuration)
                              </script>
                              <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
                          </body>
                          </html>
                          """, "text/html");
            }).ExcludeFromDescription();
        }
    }
}
