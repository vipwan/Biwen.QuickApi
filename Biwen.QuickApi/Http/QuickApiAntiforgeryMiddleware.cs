// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:33 QuickApiAntiforgeryMiddleware.cs

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// AntiforgeryMiddleware,NET8+已内部实现
    /// </summary>
    [Obsolete("NET8+已内部实现", error: false)]
    public sealed class QuickApiAntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        const string UrlEncodedFormContentType = "application/x-www-form-urlencoded";
        const string MultipartFormContentType = "multipart/form-data";

        public QuickApiAntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task Invoke(HttpContext context)
        {
            //GET请求不需要防伪验证
            if (context.Request.Method == HttpMethods.Get ||
                context.Request.Method == HttpMethods.Trace ||
                context.Request.Method == HttpMethods.Options ||
                context.Request.Method == HttpMethods.Head)
            {
                await _next(context);
                return;
            }
            var contentType = context.Request.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                await _next(context);
                return;
            }
#if NET8_0_OR_GREATER
            var requiresValidation = context.GetEndpoint()?.Metadata.GetMetadata<IAntiforgeryMetadata>()?.RequiresValidation;
            //.NET8支持屏蔽防伪验证
            if (requiresValidation is false)
            {
                await _next(context);
                return;
            }
#endif
            if (contentType.Equals(UrlEncodedFormContentType, StringComparison.OrdinalIgnoreCase) ||
                contentType.StartsWith(MultipartFormContentType, StringComparison.OrdinalIgnoreCase))
            {
                var md = context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>();
                if (md == null || md.QuickApiType == null)
                {
                    await _next(context);
                    return;
                }

                var antiforgeryApi = context.RequestServices.GetRequiredService(md.QuickApiType) as IAntiforgeryApi;
                if (antiforgeryApi?.IsAntiforgeryEnabled is true)
                {
                    try
                    {
                        await _antiforgery.ValidateRequestAsync(context);
                    }
                    catch (AntiforgeryValidationException)
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Title = "Antiforgery Validation Failed",
                            Detail = "The provided antiforgery token was meant for a different claims-based user than the current user.",
                            Status = StatusCodes.Status400BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            Instance = context.Request.Path
                        };
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(problemDetails);

                        return;
                    }
                }
            }
            await _next(context);
        }
    }
}