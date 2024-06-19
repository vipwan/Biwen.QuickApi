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
