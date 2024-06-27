namespace Biwen.QuickApi.Messaging.Sms
{
    /// <summary>
    /// ISmsSender
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="smsMessage"></param>
        /// <returns></returns>
        Task SendAsync(SmsMessage smsMessage);
    }
}
