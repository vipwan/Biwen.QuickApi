using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.FeatureManagement;
using System.Net;

namespace Biwen.QuickApi.FeatureManagement
{
    /// <summary>
    /// 处理特性的中间件,主要扩展了Endpoint & QuickApi支持
    /// </summary>
    internal class EndpointFeatureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFeatureManager _featureManager;

        public EndpointFeatureMiddleware(RequestDelegate next, IFeatureManager featureManager)
        {
            _next = next;
            _featureManager = featureManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.GetEndpoint() is { } endpoint)
            {
                if (endpoint.Metadata.GetMetadata<FeatureGateAttribute>() is { } metadata)
                {
                    var features = metadata.Features;

                    //只要有一个特性开启就可以:
                    if (metadata.RequirementType == RequirementType.Any)
                    {
                        foreach (var feature in features)
                        {
                            if (await _featureManager.IsEnabledAsync(feature))
                            {
                                await _next(context);
                                return;
                            }
                        }
                    }
                    //所有特性都必须开启:
                    else
                    {
                        foreach (var feature in features)
                        {
                            if (!await _featureManager.IsEnabledAsync(feature))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                return;
                            }
                        }
                    }
                }
            }
            await _next(context);
        }
    }
}
