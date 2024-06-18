using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                    var options = context.RequestServices.GetRequiredService<IOptions<QuickApiFeatureManagementOptions>>().Value;
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
                        context.Response.StatusCode = options.StatusCode;
                        if (options.OnErrorAsync is not null)
                        {
                            options.OnErrorAsync?.Invoke(context);
                        }
                        return;
                    }
                    //所有特性都必须开启:
                    else
                    {
                        foreach (var feature in features)
                        {
                            if (!await _featureManager.IsEnabledAsync(feature))
                            {
                                context.Response.StatusCode = options.StatusCode;
                                if (options.OnErrorAsync is not null)
                                {
                                    options.OnErrorAsync?.Invoke(context);
                                }
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
