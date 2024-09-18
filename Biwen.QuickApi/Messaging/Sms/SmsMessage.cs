// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:56:53 SmsMessage.cs

namespace Biwen.QuickApi.Messaging.Sms;

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
