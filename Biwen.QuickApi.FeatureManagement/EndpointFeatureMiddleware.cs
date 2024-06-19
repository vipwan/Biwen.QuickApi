using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Biwen.QuickApi.FeatureManagement
{
    /// <summary>
    /// 处理特性的中间件,主要扩展了Endpoint & QuickApi支持
    /// </summary>
    internal class EndpointFeatureMiddleware
    {
        private readonly RequestDelegate _next;

        public EndpointFeatureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.GetEndpoint() is { } endpoint)
            {
                //这里不处理PageRouteMetadata,因为PageRouteMetadata是用于RazorPage的,不是用于MinimalApi的
                if (endpoint.Metadata.OfType<PageRouteMetadata>().Any())
                {
                    await _next(context);
                    return;
                }

                //不处理Mvc Controller
                if (endpoint.Metadata.OfType<ControllerActionDescriptor>().Any())
                {
                    await _next(context);
                    return;
                }

                if (endpoint.Metadata.GetMetadata<FeatureGateAttribute>() is { } metadata)
                {
                    //IFeatureManagerSnapshot 比 IFeatureManager 多了一层缓存封装,性能更好
                    //IFeatureManagerSnapshot 适用于作用域请求,IFeatureManager 适用于单例请求. 因此IFeatureManagerSnapshot不要在构造器中注入
                    var _featureManager = context.RequestServices.GetRequiredService<IFeatureManagerSnapshot>();
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
                        if (options.OnErrorAsync is { } errorHandler)
                        {
                            errorHandler.Invoke(context);
                        }
                        else
                        {
                            //返回规范的Result.Problem:
                            await Results.Problem(statusCode: options.StatusCode).ExecuteAsync(context);
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
                                if (options.OnErrorAsync is { } errorHandler)
                                {
                                    errorHandler.Invoke(context);
                                }
                                else
                                {
                                    //返回规范的Result.Problem:
                                    await Results.Problem(statusCode: options.StatusCode).ExecuteAsync(context);
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