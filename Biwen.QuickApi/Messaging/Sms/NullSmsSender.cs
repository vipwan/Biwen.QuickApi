// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:56:43 NullSmsSender.cs

namespace Biwen.QuickApi.Messaging.Sms;

/// <summary>
/// NullSmsSender
/// </summary>
public sealed class NullSmsSender(ILogger<NullSmsSender> logger) : ISmsSender
{
    /// <summary>
    /// NullSmsSender
    /// </summary>
    public const string Name = nameof(NullSmsSender);

    /// <inheritdoc/>
    public string KeyedName => Name;

    public Task SendAsync(SmsMessage smsMessage)
    {
        logger.LogWarning($"SMS Sending was not implemented! Using {nameof(NullSmsSender)}:");
        logger.LogWarning("Phone Number : " + smsMessage.PhoneNumber);
        logger.LogWarning("SMS Text     : " + smsMessage.Text);

        return Task.CompletedTask;
    }
}