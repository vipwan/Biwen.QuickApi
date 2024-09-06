// Licensed to the Biwen.QuickApi.FeatureManagement under one or more agreements.
// The Biwen.QuickApi.FeatureManagement licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.FeatureManagement Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:58:55 HttpContextExtensions.cs

using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Metadata;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.FeatureManagement
{
    internal static class HttpContextExtensions
    {
        /// <summary>
        /// 用于判断当前请求是否是QuickApi请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsQuickApi(this HttpContext context)
        {
            return
                context.GetEndpoint()?.Metadata.GetMetadata<QuickApiAttribute>() is not null ||
                context.GetEndpoint()?.Metadata.GetMetadata<QuickApiEndpointMetadata>() is not null;
        }

    }
}
