namespace NotificationService.Services;

public interface IEmailService
{
    public Task SendAsync(string toEmail, string subject, string htmlMessage);
}