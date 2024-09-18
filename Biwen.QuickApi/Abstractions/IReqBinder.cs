// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IReqBinder.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Abstractions;

/// <summary>
/// IReqBinder绑定器约定接口,当前同时支持QuickApi和MinimalApi
/// 请注意IReqBinder,会导致Swagger无法生成正确的Schema,请务必重写<see cref="BaseQuickApi.HandlerBuilder(RouteHandlerBuilder)"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IReqBinder<T> where T : class, new()
{
    static abstract ValueTask<T> BindAsync(HttpContext context, ParameterInfo parameter = null!);
}