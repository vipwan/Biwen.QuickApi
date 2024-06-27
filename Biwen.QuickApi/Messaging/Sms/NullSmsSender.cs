namespace Biwen.QuickApi.Messaging.Sms
{
    /// <summary>
    /// NullSmsSender
    /// </summary>
    public sealed class NullSmsSender(ILogger<NullSmsSender> logger) : ISmsSender
    {
        public Task SendAsync(SmsMessage smsMessage)
        {
            logger.LogWarning($"SMS Sending was not implemented! Using {nameof(NullSmsSender)}:");
            logger.LogWarning("Phone Number : " + smsMessage.PhoneNumber);
            logger.LogWarning("SMS Text     : " + smsMessage.Text);

            return Task.CompletedTask;
        }
    }
}