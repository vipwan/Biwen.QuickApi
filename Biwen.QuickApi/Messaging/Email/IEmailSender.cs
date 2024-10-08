﻿// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:20 IEmailSender.cs

using System.Net.Mail;

namespace Biwen.QuickApi.Messaging.Email;
public class EmailAttachment
{
    public string? Name { get; set; }

    public byte[]? File { get; set; }
}


public class AdditionalEmailSendingArgs
{
    public List<string>? CC { get; set; }

    public List<EmailAttachment>? Attachments { get; set; }
}

/// <summary>
/// This service can be used simply sending emails.
/// </summary>
public interface IEmailSender : INamed<string>
{

    /// <summary>
    /// Sends an email.
    /// </summary>
    Task SendAsync(
        string to,
        string? subject,
        string? body,
        bool isBodyHtml = true,
        AdditionalEmailSendingArgs? additionalEmailSendingArgs = null
    );

    /// <summary>
    /// Sends an email.
    /// </summary>
    Task SendAsync(
        string from,
        string to,
        string? subject,
        string? body,
        bool isBodyHtml = true,
        AdditionalEmailSendingArgs? additionalEmailSendingArgs = null
    );

    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="mail">Mail to be sent</param>
    /// <param name="normalize">
    /// Should normalize email?
    /// If true, it sets sender address/name if it's not set before and makes mail encoding UTF-8.
    /// </param>
    Task SendAsync(
        MailMessage mail,
        bool normalize = true
    );

    /// <summary>
    /// Adds an email to queue to send via background jobs.
    /// </summary>
    Task QueueAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        AdditionalEmailSendingArgs? additionalEmailSendingArgs = null
    );

    /// <summary>
    /// Adds an email to queue to send via background jobs.
    /// </summary>
    Task QueueAsync(
        string from,
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        AdditionalEmailSendingArgs? additionalEmailSendingArgs = null
    );
}
