using Infrastructure.Services.Messaging;
using Newtonsoft.Json;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.Users;

namespace SupportTickets.WebApi.Services;

public class UsersWorker : IHostedService
{
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly IUsersRepository _usersRepository;

    public UsersWorker(
        IRabbitMqConsumer rabbitMqConsumer,
        IUsersRepository usersRepository)
    {
        _rabbitMqConsumer = rabbitMqConsumer;
        _usersRepository = usersRepository;
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
        await _usersRepository.InsertAsync(user);
    }

    private async Task HandleUserUpdatedAsync(string json)
    {
        var user = JsonConvert.DeserializeObject<User>(json)!;
        await _usersRepository.UpdateAsync(user);
    }

    private async Task HandleUserDeletedAsync(string json)
    {
        var user = JsonConvert.DeserializeObject<User>(json)!;
        await _usersRepository.DeleteAsync(user.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMqConsumer.Stop();
        return Task.CompletedTask;
    }
}