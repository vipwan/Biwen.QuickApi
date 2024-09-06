// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:48 RequestBindService.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// 绑定服务
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    internal class RequestBindService(IHttpContextAccessor httpContextAccessor)
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> _reqBinderCache = new();

        public async ValueTask<dynamic?> BindAsync(Type apiType)
        {
            if (httpContextAccessor?.HttpContext is null)
            {
                return null;
            }

            var methodInfo = _reqBinderCache.GetOrAdd(apiType, (apiType) =>
            {
                var methodName = nameof(IReqBinder<EmptyRequest>.BindAsync);
                var api = httpContextAccessor.HttpContext.RequestServices.GetRequiredService(apiType);
                return (((dynamic)api).ReqBinder).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            });

            var req = await (dynamic)methodInfo.Invoke(null, [httpContextAccessor.HttpContext, null])!;
            return req;
        }
    }
}