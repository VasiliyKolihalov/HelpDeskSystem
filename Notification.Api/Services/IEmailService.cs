namespace Notification.Api.Services;

public interface IEmailService
{
    public Task SendAsync(string toEmail, string toName, string subject, string htmlMessage);
}