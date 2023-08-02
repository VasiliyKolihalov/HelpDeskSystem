using NotificationService.Models.Messaging;

namespace NotificationService.Services;

public interface INotificationsService
{
    public Task SendConfirmCodeEmailAsync(RequestEmailConfirm requestEmailConfirm);
}