// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:56:57 SmsSenderExtensions.cs

namespace Biwen.QuickApi.Messaging.Sms;

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
