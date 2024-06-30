using Biwen.QuickApi.Infrastructure.DependencyInjection;

namespace Biwen.QuickApi.Messaging.Sms
{
    /// <summary>
    /// ISmsSender
    /// </summary>
    public interface ISmsSender : INamed<string>
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="smsMessage"></param>
        /// <returns></returns>
        Task SendAsync(SmsMessage smsMessage);

    }
}
