using Infrastructure.Services.Messaging;
using Microsoft.Extensions.Hosting;

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
        // no action required yet
        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // no action required yet
        return Task.CompletedTask;
    }
}