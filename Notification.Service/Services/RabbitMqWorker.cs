using Infrastructure.Services.Messaging;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NotificationService.Models.Messaging;

namespace NotificationService.Services;

public class RabbitMqWorker : IHostedService
{
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly INotificationsService _notificationsService;

    public RabbitMqWorker(
        IRabbitMqConsumer rabbitMqConsumer, 
        INotificationsService notificationsService)
    {
        _rabbitMqConsumer = rabbitMqConsumer;
        _notificationsService = notificationsService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _rabbitMqConsumer.Start(new Dictionary<string, Func<string, Task>>
        {
            ["notifications.requested_email_confirm_code"] = HandleEmailConfirm
        });
        await Task.CompletedTask;
    }
    
    private async Task HandleEmailConfirm(string json)
    {
        var model = JsonConvert.DeserializeObject<RequestEmailConfirm>(json);
        await _notificationsService.SendConfirmCodeEmailAsync(model!);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMqConsumer.Stop();
        return Task.CompletedTask;
    }
}