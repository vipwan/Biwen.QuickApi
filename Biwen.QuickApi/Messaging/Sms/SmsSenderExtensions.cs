namespace Biwen.QuickApi.Messaging.Sms
{
    /// <summary>
    /// ISmsSender扩展
    /// </summary>
    public static class SmsSenderExtensions
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="smsSender"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Task SendAsync(this ISmsSender smsSender, string phoneNumber, string text)
        {
            ArgumentNullException.ThrowIfNull(smsSender);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(phoneNumber);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(text);

            return smsSender.SendAsync(new SmsMessage(phoneNumber, text));
        }
    }
}
