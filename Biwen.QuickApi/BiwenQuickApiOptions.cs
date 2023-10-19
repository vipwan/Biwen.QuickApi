
using System.Text.Json;

namespace Biwen.QuickApi
{

    public class BiwenQuickApiOptions
    {
        /// <summary>
        /// 全局路径前缀
        /// </summary>
        public string RoutePrefix { get; set; } = "api";

        /// <summary>
        /// 是否开启ProblemDetails,默认:false 不开启 ,开发阶段调试 推荐开启
        /// 如果开启你也可以自行调用配置更多选项<see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection, Action{Microsoft.AspNetCore.Http.ProblemDetailsOptions}?)"/>
        /// </summary>
        public bool AddProblemDetails { get; set; } = false;

    }
}