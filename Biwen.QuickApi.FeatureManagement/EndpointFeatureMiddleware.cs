// Licensed to the Biwen.QuickApi.FeatureManagement under one or more agreements.
// The Biwen.QuickApi.FeatureManagement licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.FeatureManagement Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:58:39 EndpointFeatureMiddleware.cs

using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Biwen.QuickApi.FeatureManagement;

/// <summary>
/// 处理特性的中间件,主要扩展了Endpoint & QuickApi支持
/// </summary>
internal sealed class EndpointFeatureMiddleware
{
    private readonly RequestDelegate _next;

    public EndpointFeatureMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        // 快速跳过不需处理的端点类型
        if (ShouldSkipEndpoint(endpoint))
        {
            await _next(context);
            return;
        }

        // 获取特性门控属性
        var metadata = endpoint.Metadata.GetMetadata<FeatureGateAttribute>();
        if (metadata is null)
        {
            await _next(context);
            return;
        }

        // 处理特性门控逻辑
        var featureManager = context.RequestServices.GetRequiredService<IFeatureManagerSnapshot>();
        var options = context.RequestServices.GetRequiredService<IOptions<QuickApiFeatureManagementOptions>>().Value;

        bool isAllowed = metadata.RequirementType == RequirementType.Any
            ? await IsAnyFeatureEnabledAsync(metadata.Features, featureManager)
            : await AreAllFeaturesEnabledAsync(metadata.Features, featureManager);

        if (isAllowed)
        {
            await _next(context);
            return;
        }

        // 处理未通过特性门控的请求
        await HandleFeatureNotAllowedAsync(context, options);
    }

    private static bool ShouldSkipEndpoint(Endpoint endpoint)
    {
        return endpoint.Metadata.OfType<PageRouteMetadata>().Any() ||
               endpoint.Metadata.OfType<ControllerActionDescriptor>().Any();
    }

    private static async Task<bool> IsAnyFeatureEnabledAsync(IEnumerable<string> features, IFeatureManagerSnapshot featureManager)
    {
        foreach (var feature in features)
        {
            if (await featureManager.IsEnabledAsync(feature))
            {
                return true;
            }
        }
        return false;
    }

    private static async Task<bool> AreAllFeaturesEnabledAsync(IEnumerable<string> features, IFeatureManagerSnapshot featureManager)
    {
        foreach (var feature in features)
        {
            if (!await featureManager.IsEnabledAsync(feature))
            {
                return false;
            }
        }
        return true;
    }

    private static async Task HandleFeatureNotAllowedAsync(HttpContext context, QuickApiFeatureManagementOptions options)
    {
        context.Response.StatusCode = options.StatusCode;

        if (options.OnErrorAsync is { } errorHandler)
        {
            await Task.Run(() => errorHandler.Invoke(context));
        }
        else
        {
            await Results.Problem(statusCode: options.StatusCode).ExecuteAsync(context);
        }
    }
}
