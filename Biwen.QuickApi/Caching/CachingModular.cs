// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 CachingModular.cs

using Biwen.QuickApi.Caching.Abstractions;
using Biwen.QuickApi.Caching.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Caching;

[CoreModular]
internal class CachingModular : ModularBase
{
    /// <summary>
    /// 缓存模块比HttpModular先启动
    /// </summary>
    public override int Order => base.Order - 1;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();

        services.AddOutputCache();
        services.AddResponseCaching();

        //注入IProxyCache,默认使用MemoryCache
        services.AddScoped<IProxyCache, MemoryProxyCache>();
        //注入Caching代理
        services.TryAddScoped(typeof(CachingProxyFactory<>));
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseOutputCache();
        app.UseResponseCaching();
    }
}