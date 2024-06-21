namespace Biwen.QuickApi.Telemetry
{
    /// <summary>
    /// TelemetryOptions 配置项
    /// </summary>
    public class TelemetryOptions
    {

        public const string Key = "BiwenQuickApi:Telemetry";

        /// <summary>
        /// 是否启用 默认: false
        /// </summary>
        public bool Enable { get; set; } = false;


        /// <summary>
        /// 服务监控采样间隔 默认: 15s
        /// </summary>
        public uint SamplingInterval { get; set; } = 15;
    }
}
