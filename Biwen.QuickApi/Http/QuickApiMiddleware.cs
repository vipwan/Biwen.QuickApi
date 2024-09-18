// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:44 QuickApiMiddleware.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http;

/// <summary>
/// QuickApiMiddleware
/// </summary>
internal sealed class QuickApiMiddleware
{
    private static readonly string? AssemblyName = typeof(ServiceRegistration).Assembly.GetName().Name;
    private static readonly string version = $"{typeof(ServiceRegistration).Assembly.GetName().Version}";

    private readonly RequestDelegate _next;
    public QuickApiMiddleware(RequestDelegate next)
    {
        _next = next;
    }
#pragma warning disable GEN051 // 将异步方法名改为以Async结尾
    public async Task Invoke(HttpContext context)
#pragma warning restore GEN051 // 将异步方法名改为以Async结尾
    {
        var addHeader = () =>
        {
            //PoweredBy
            context.Response.Headers.XPoweredBy = AssemblyName;
            //Version
            context.Response.Headers.TryAdd($"X-{nameof(QuickApi)}-Version", version);
        };

        if (context.GetEndpoint()?.Metadata.OfType<QuickApiEndpointMetadata>().Any() is true)
        {
            addHeader();
            await _next(context);
            return;
        }

        if (context.GetEndpoint()?.Metadata.GetMetadata<QuickApiMetadata>() is { QuickApiType: not null } md)
        {
            addHeader();
            if (context.RequestServices.GetService(md.QuickApiType) is IQuickApiMiddlewareHandler handler)
            {
                await handler.BeforeAsync(context);
                await _next(context);
                await handler.AfterAsync(context);
                return;
            }
        }

        await _next(context);
    }
}