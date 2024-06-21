namespace Biwen.QuickApi.Telemetry
{
    public static class Constant
    {
        /// <summary>
        /// ActivitySourceName
        /// </summary>
        public const string OpenTelemetryActivitySourceName = "Biwen.QuickApi";

        public const string OpenTelemetryVersion = "1.0.0";

        /// <summary>
        /// CPU 使用比例 %
        /// </summary>
        public const string CpuUsedPercentage = "CpuUsedPercentage";

        /// <summary>
        /// 内存使用比例 %
        /// </summary>
        public const string MemoryUsedPercentage = "MemoryUsedPercentage";

        /// <summary>
        /// 内存已使用量 bytes
        /// </summary>
        public const string MemoryUsedInBytes = "MemoryUsedInBytes";

        /// <summary>
        /// 内存总计 bytes
        /// </summary>
        public const string MaximumMemoryInBytes = "MaximumMemoryInBytes";

    }
}