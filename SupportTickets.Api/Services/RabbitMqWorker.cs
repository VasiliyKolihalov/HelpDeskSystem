using Infrastructure.Services.Messaging;
using Newtonsoft.Json;
using SupportTickets.Api.Models.Users;
using SupportTickets.Api.Services.Users;

namespace SupportTickets.Api.Services;

public class RabbitMqWorker : IHostedService
{
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly IUsersService _usersService;

    public RabbitMqWorker(
        IRabbitMqConsumer rabbitMqConsumer,
        IUsersService usersService)
    {
        _rabbitMqConsumer = rabbitMqConsumer;
        _usersService = usersService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _rabbitMqConsumer.Start(new Dictionary<string, Func<string, Task>>
        {
            ["users.created"] = HandleUserCreatedAsync,
            ["users.updated"] = HandleUserUpdatedAsync,
            ["users.deleted"] = HandleUserDeletedAsync
        });
        await Task.CompletedTask;
    }

    private async Task HandleUserCreatedAsync(string json)
    {
        var user = JsonConvert.DeserializeObject<User>(json)!;
        await _usersService.CreateAsync(user);
    }

    private async Task HandleUserUpdatedAsync(string json)
    {
        var user = JsonConvert.DeserializeObject<User>(json)!;
        await _usersService.UpdateAsync(user);
    }

    private async Task HandleUserDeletedAsync(string json)
    {
        var user = JsonConvert.DeserializeObject<User>(json)!;
        await _usersService.DeleteAsync(user.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMqConsumer.Stop();
        return Task.CompletedTask;
    }
}