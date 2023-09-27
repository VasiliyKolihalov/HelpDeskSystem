using Notification.Api.Models.Messaging;
using Scriban;

namespace Notification.Api.Services;

public class NotificationsService : INotificationsService
{
    private readonly IEmailService _emailService;

    public NotificationsService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendConfirmCodeEmailAsync(RequestEmailConfirm requestEmailConfirm)
    {
        string templateString = await File.ReadAllTextAsync("Templates/EmailConfirm.html");
        Template template = Template.Parse(templateString);
        string message = await template.RenderAsync(model: new
        {
            first_name = requestEmailConfirm.FirstName,
            confirm_code = requestEmailConfirm.ConfirmCode
        });
        
        await _emailService.SendAsync(
            toEmail: requestEmailConfirm.Email,
            toName: requestEmailConfirm.FirstName,
            subject: "Email confirm",
            htmlMessage: message);
    }
}