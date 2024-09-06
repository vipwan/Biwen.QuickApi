// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:08 AliyunSmsSender.cs

using Biwen.QuickApi.Messaging.Sms;
using Microsoft.Extensions.Options;

using AliyunClient = AlibabaCloud.SDK.Dysmsapi20170525.Client;
using AliyunConfig = AlibabaCloud.OpenApiClient.Models.Config;
using AliyunSendSmsRequest = AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest;

namespace Biwen.QuickApi.Messaging.AliyunSms;

/// <summary>
/// 阿里云短信发送器
/// </summary>
public class AliyunSmsSender : ISmsSender
{
    private readonly AliyunSmsOptions _aliyunSmsOptions;

    public AliyunSmsSender(IOptions<AliyunSmsOptions> options)
    {
        _aliyunSmsOptions = options.Value;
    }

    public const string Name = nameof(AliyunSmsSender);

    public virtual string KeyedName => Name;

    /// <inheritdoc/>
    public async Task SendAsync(SmsMessage smsMessage)
    {
        var client = CreateClient();

        await client.SendSmsAsync(new AliyunSendSmsRequest
        {
            PhoneNumbers = smsMessage.PhoneNumber,
            SignName = smsMessage.Properties.GetOrDefault("SignName") as string,
            TemplateCode = smsMessage.Properties.GetOrDefault("TemplateCode") as string,
            TemplateParam = smsMessage.Text
        });
    }

    protected virtual AliyunClient CreateClient()
    {
        return new(new AliyunConfig
        {
            AccessKeyId = _aliyunSmsOptions.AccessKeyId,
            AccessKeySecret = _aliyunSmsOptions.AccessKeySecret,
            Endpoint = _aliyunSmsOptions.EndPoint
        });
    }
}
