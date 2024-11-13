﻿// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:15 HttpModular.cs

using Biwen.QuickApi.Infrastructure.Html;
using Microsoft.AspNetCore.Authorization;

namespace Biwen.QuickApi.Http;

[CoreModular, PreModular<HtmlSanitizerModular>]
internal class HttpModular(IServiceProvider serviceProvider) : ModularBase
{
    public override int Order => base.Order;

    public override void ConfigureServices(IServiceCollection services)
    {
        //401,403不做跳转,重写AuthorizationMiddlewareResultHandler
        services.AddActivatedSingleton<IAuthorizationMiddlewareResultHandler, QuickApiAuthorizationMiddlewareResultHandler>();
        var useQuickApiExceptionResultBuilder = serviceProvider.GetRequiredService<IOptions<BiwenQuickApiOptions>>().Value.UseQuickApiExceptionResultBuilder;
        //默认的异常返回构造器
        services.AddIf(useQuickApiExceptionResultBuilder, sp =>
        {
            services.AddActivatedSingleton<IQuickApiExceptionResultBuilder, DefaultExceptionResultBuilder>();
        });

        //注册QuickApi
        foreach (var api in Apis) services.AddScoped(api);

        //注册绑定服务
        services.AddScoped<RequestBindService>();

        //注册BlazorRendererService
        services.AddScoped<BlazorRendererService>();
    }

    static readonly Type InterfaceQuickApi = typeof(IQuickApi<,>);
    static readonly Lock _lock = new();//锁
    static bool IsToGenericInterface(Type type, Type baseInterface)
    {
        if (type == null) return false;
        if (baseInterface == null) return false;
        return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == baseInterface);
    }

    static IEnumerable<Type> _apis = null!;
    /// <summary>
    /// 所有的QuickApi
    /// </summary>
    public static IEnumerable<Type> Apis
    {
        get
        {
            lock (_lock)
                return _apis ??= ASS.InAllRequiredAssemblies.Where(x =>
                !x.IsAbstract && x.IsPublic && x.IsClass && IsToGenericInterface(x, InterfaceQuickApi));
        }
    }

}