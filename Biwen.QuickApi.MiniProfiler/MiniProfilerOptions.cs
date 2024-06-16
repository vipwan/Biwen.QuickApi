using StackExchange.Profiling;

namespace Biwen.QuickApi.MiniProfiler
{
    /// <summary>
    /// MiniProfiler配置选项
    /// </summary>
    public class WrapMiniProfilerOptions : MiniProfilerOptions
    {
        /// <summary>
        /// 配置文件中的定位Key
        /// </summary>
        public const string Key = "BiwenQuickApi:MiniProfiler";

        /// <summary>
        /// 是否启用MiniProfiler
        /// </summary>
        public bool Enabled { get; set; } = false;

    }
}