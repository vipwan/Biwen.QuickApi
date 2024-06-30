namespace Biwen.QuickApi.Messaging.AliyunSms
{
    /// <summary>
    /// 阿里云短信配置
    /// </summary>
    public class AliyunSmsOptions
    {
        /// <summary>
        /// Secret
        /// </summary>
        public string AccessKeySecret { get; set; } = default!;
        /// <summary>
        /// AccessKeyId
        /// </summary>
        public string AccessKeyId { get; set; } = default!;
        /// <summary>
        /// Endpoint
        /// </summary>
        public string EndPoint { get; set; } = default!;

    }
}
