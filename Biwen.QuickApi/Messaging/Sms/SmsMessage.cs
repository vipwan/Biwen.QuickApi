namespace Biwen.QuickApi.Messaging.Sms
{
    /// <summary>
    /// 短信消息
    /// </summary>
    public class SmsMessage
    {
        public string PhoneNumber { get; }

        public string Text { get; }

        public IDictionary<string, object> Properties { get; }

        public SmsMessage(string phoneNumber, string text)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(phoneNumber);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(text);

            PhoneNumber = phoneNumber;
            Text = text;
            Properties = new Dictionary<string, object>();
        }
    }
}
