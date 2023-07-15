using Infrastructure.Models;
using Infrastructure.Services;
using Infrastructure.Services.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessagePublisher(
        this IServiceCollection @this,
        string configurationSection)
    {
        @this
            .AddOptions<RabbitMqConfiguration>()
            .BindConfiguration(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return @this.AddSingleton<ConnectionFactory>(_ =>
        {
            var configuration = _.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
            return new ConnectionFactory
            {
                HostName = configuration.Host,
                UserName = configuration.UserName,
                Password = configuration.Password,
                Port = configuration.Port,
                AutomaticRecoveryEnabled = configuration.AutomaticRecovery
            };
        }).AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>(_ =>
        {
            var publisher = new RabbitMqPublisher(
                rabbitMqConfiguration: _.GetRequiredService<IOptions<RabbitMqConfiguration>>(),
                connectionFactory: _.GetRequiredService<ConnectionFactory>(),
                logger: _.GetRequiredService<ILogger<RabbitMqPublisher>>());
            publisher.Connect();
            return publisher;
        });
    }
    
    public static IServiceCollection AddRabbitMqMessageConsumer(
        this IServiceCollection @this,
        string configurationSection)
    {
        @this
            .AddOptions<RabbitMqConfiguration>()
            .BindConfiguration(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return @this.AddSingleton<ConnectionFactory>(_ =>
        {
            var configuration = _.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
            return new ConnectionFactory
            {
                HostName = configuration.Host,
                UserName = configuration.UserName,
                Password = configuration.Password,
                Port = configuration.Port,
                AutomaticRecoveryEnabled = configuration.AutomaticRecovery
            };
        }).AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>(_ => new RabbitMqConsumer(
            rabbitMqConfiguration: _.GetRequiredService<IOptions<RabbitMqConfiguration>>(),
            connectionFactory: _.GetRequiredService<ConnectionFactory>(),
            logger: _.GetRequiredService<ILogger<RabbitMqConsumer>>()));
    }
    
    
}