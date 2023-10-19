namespace Biwen.QuickApi
{
    using Microsoft.AspNetCore.Http;

    public class BiwenQuickApiOptions
    {
        /// <summary>
        /// 全局路径前缀
        /// </summary>
        public string RoutePrefix { get; set; } = "api";
    }
}