using System.Reflection;
using FluentMigrator.Runner;
using Infrastructure.Models;
using Infrastructure.Services.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessagePublisher(
        this IServiceCollection @this,
        IConfigurationSection configurationSection)
    {
        @this
            .AddOptions<RabbitMqOptions>()
            .Bind(configurationSection)
            .ValidateOnStart();

        return @this.AddSingleton<ConnectionFactory>(_ =>
        {
            var configuration = _.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
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
                rabbitMqOptions: _.GetRequiredService<IOptions<RabbitMqOptions>>(),
                connectionFactory: _.GetRequiredService<ConnectionFactory>(),
                logger: _.GetRequiredService<ILogger<RabbitMqPublisher>>());
            publisher.Connect();
            return publisher;
        });
    }

    public static IServiceCollection AddRabbitMqMessageConsumer(
        this IServiceCollection @this,
        IConfigurationSection configurationSection)
    {
        @this
            .AddOptions<RabbitMqOptions>()
            .Bind(configurationSection)
            .ValidateOnStart();

        return @this.AddSingleton<ConnectionFactory>(_ =>
        {
            var configuration = _.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            return new ConnectionFactory
            {
                HostName = configuration.Host,
                UserName = configuration.UserName,
                Password = configuration.Password,
                Port = configuration.Port,
                AutomaticRecoveryEnabled = configuration.AutomaticRecovery
            };
        }).AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>(_ => new RabbitMqConsumer(
            rabbitMqOptions: _.GetRequiredService<IOptions<RabbitMqOptions>>(),
            connectionFactory: _.GetRequiredService<ConnectionFactory>(),
            logger: _.GetRequiredService<ILogger<RabbitMqConsumer>>()));
    }

    public static IServiceCollection AddFluentMigrationForPostgres(
        this IServiceCollection @this,
        string connectionString,
        Assembly executingAssembly)
    {
        return @this.AddLogging(_ => _.AddFluentMigratorConsole())
            .AddFluentMigratorCore()
            .ConfigureRunner(_ => _.AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(executingAssembly).For.Migrations()
                .ConfigureGlobalProcessorOptions(opt => { opt.ProviderSwitches = "Force Quote=false"; }));
    }
}