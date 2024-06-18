using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.FeatureManagement
{
    /// <summary>
    /// QuickApiFeatureManagement配置项
    /// </summary>
    public class QuickApiFeatureManagementOptions
    {
        /// <summary>
        /// 失败返回的状态码,默认:404
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status404NotFound;

        /// <summary>
        /// 失败时的处理.
        /// </summary>
        public Action<HttpContext>? OnErrorAsync { get; set; } = null;

    }
}
