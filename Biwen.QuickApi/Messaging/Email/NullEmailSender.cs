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
