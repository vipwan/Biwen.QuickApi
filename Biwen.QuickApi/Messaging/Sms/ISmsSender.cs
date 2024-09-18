// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:56:36 ISmsSender.cs

namespace Biwen.QuickApi.Messaging.Sms;

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
