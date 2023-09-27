using Notification.Api.Models.Messaging;

namespace Notification.Api.Services;

public interface INotificationsService
{
    public Task SendConfirmCodeEmailAsync(RequestEmailConfirm requestEmailConfirm);
}