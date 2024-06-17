using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

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