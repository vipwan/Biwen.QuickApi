// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:25 NullEmailSender.cs

using System.Net.Mail;

namespace Biwen.QuickApi.Messaging.Email;

public class NullEmailSender : IEmailSender
{
    public const string Name = nameof(NullEmailSender);

    /// <inheritdoc/>
    public string KeyedName => Name;


    public Task QueueAsync(string to, string subject, string body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        return Task.CompletedTask;
    }

    public Task QueueAsync(string from, string to, string subject, string body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        return Task.CompletedTask;
    }

    public Task SendAsync(string to, string? subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        return Task.CompletedTask;
    }

    public Task SendAsync(string from, string to, string? subject, string? body, bool isBodyHtml = true, AdditionalEmailSendingArgs? additionalEmailSendingArgs = null)
    {
        return Task.CompletedTask;
    }

    public Task SendAsync(MailMessage mail, bool normalize = true)
    {
        return Task.CompletedTask;
    }
}
