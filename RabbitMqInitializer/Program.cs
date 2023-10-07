using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMqInitializer;

RabbitMqOptions rabbitMqOptions = GetRabbitMqOptions();
ILogger logger = CreateLogger();
IModel model = CreateModal(rabbitMqOptions, logger);

DeclareExchanges(model);
DeclareQueues(model);
BindQueues(model);

model.Dispose();
return;

static RabbitMqOptions GetRabbitMqOptions()
{
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    return configuration.GetSection("RabbitMqOptions").GetAndValidate<RabbitMqOptions>();
}

static ILogger CreateLogger()
{
    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
    return loggerFactory.CreateLogger<Program>();
}

static IModel CreateModal(RabbitMqOptions rabbitMqOptions, ILogger logger)
{
    var connectionFactory = new ConnectionFactory
    {
        HostName = rabbitMqOptions.Host,
        UserName = rabbitMqOptions.UserName,
        Password = rabbitMqOptions.Password,
        Port = rabbitMqOptions.Port!.Value,
        AutomaticRecoveryEnabled = rabbitMqOptions.AutomaticRecovery!.Value
    };

    IModel model = null!;
    Policy
        .Handle<Exception>()
        .WaitAndRetry(
            retryCount: rabbitMqOptions.ConnectionRetryCount!.Value,
            sleepDurationProvider: _ => rabbitMqOptions.ConnectionRetrySleepDuration!.Value,
            onRetry: (exception, timeSpan) =>
            {
                logger.LogError(
                    exception: exception,
                    message: "Error connecting to RabbitMQ. Retrying in {TimeSpan} sec",
                    args: timeSpan);
            })
        .Execute(() =>
        {
            IConnection connection = connectionFactory.CreateConnection();
            model = connection.CreateModel();
            connection.Dispose();
        });
    return model;
}

static void DeclareExchanges(IModel model)
{
    model.ExchangeDeclare(
        exchange: "users",
        type: ExchangeType.Fanout,
        durable: true);
    model.ExchangeDeclare(
        exchange: "notifications",
        type: ExchangeType.Fanout,
        durable: true);
}

static void DeclareQueues(IModel model)
{
    model.QueueDeclare(
        queue: "users.created",
        durable: true,
        exclusive: false,
        autoDelete: false);
    model.QueueDeclare(
        queue: "users.deleted",
        durable: true,
        exclusive: false,
        autoDelete: false);
    model.QueueDeclare(
        queue: "users.updated",
        durable: true,
        exclusive: false,
        autoDelete: false);
    model.QueueDeclare(
        queue: "notifications.requested_email_confirm_code",
        durable: true,
        exclusive: false,
        autoDelete: false);
}

static void BindQueues(IModel model)
{
    model.QueueBind(
        queue: "users.created",
        exchange: "users",
        routingKey: "users.created");
    model.QueueBind(
        queue: "users.deleted",
        exchange: "users",
        routingKey: "users.deleted");
    model.QueueBind(
        queue: "users.updated",
        exchange: "users",
        routingKey: "users.updated");
    model.QueueBind(
        queue: "notifications.requested_email_confirm_code",
        exchange: "notifications",
        routingKey: "notifications.requested_email_confirm_code");
}