using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Notification.Api.Models.Options;

namespace Notification.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;

    public EmailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlMessage)
    {
        if (toEmail == null) throw new ArgumentNullException(nameof(toEmail));
        if (toName == null) throw new ArgumentNullException(nameof(toName));
        if (subject == null) throw new ArgumentNullException(nameof(subject));
        if (htmlMessage == null) throw new ArgumentNullException(nameof(htmlMessage));

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(name: _emailOptions.SenderName, address: _emailOptions.SenderEmail));
        emailMessage.To.Add(new MailboxAddress(name: toName, address: toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(TextFormat.Html)
        {
            Text = htmlMessage
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            host: _emailOptions.Host,
            port: _emailOptions.Port!.Value,
            useSsl: _emailOptions.IsNeedSsl!.Value);
        await client.AuthenticateAsync(userName: _emailOptions.SenderEmail, password: _emailOptions.SenderPassword);
        await client.SendAsync(emailMessage);

        await client.DisconnectAsync(quit: true);
    }
}