
using System.Text.Json;

namespace Biwen.QuickApi
{

    [Obsolete("Gen版本不支持配置Options,请使用原生支持", false)]
    public class BiwenQuickApiOptions
    {
        /// <summary>
        /// 全局路径前缀
        /// </summary>
        public string RoutePrefix { get; set; } = "api";

        /// <summary>
        /// Json序列化配置项
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,//默认使用驼峰
            //PropertyNameCaseInsensitive = true,
            //IgnoreReadOnlyProperties = true,
            //WriteIndented = true,
        };
    }
}