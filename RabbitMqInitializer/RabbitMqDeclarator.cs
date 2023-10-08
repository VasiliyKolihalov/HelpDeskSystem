using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;

namespace RabbitMqInitializer;

public sealed class RabbitMqDeclarator : IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly ILogger _logger;

    private IConnection? _connection;
    private IModel? _model;

    public RabbitMqDeclarator(
        RabbitMqOptions rabbitMqOptions,
        ILogger logger)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.Host,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password,
            Port = rabbitMqOptions.Port!.Value,
            AutomaticRecoveryEnabled = rabbitMqOptions.AutomaticRecovery!.Value
        };
        _rabbitMqOptions = rabbitMqOptions;
        _logger = logger;
    }

    public void Connect()
    {
        Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: _rabbitMqOptions.ConnectionRetryCount!.Value,
                sleepDurationProvider: _ => _rabbitMqOptions.ConnectionRetrySleepDuration!.Value,
                onRetry: (exception, timeSpan) =>
                {
                    _logger.LogError(
                        exception: exception,
                        message: "Error connecting to RabbitMQ. Retrying in {TimeSpan} sec",
                        args: timeSpan);
                })
            .Execute(() =>
            {
                _connection = _connectionFactory.CreateConnection();
                _model = _connection.CreateModel();
            });
    }

    public void DeclareExchange(string exchangeName)
    {
        _model.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true);
        _logger.LogInformation(message: "Exchange {ExchangeName} declared", args: exchangeName);
    }

    public void DeclareQueue(string queueName)
    {
        _model.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);
        _logger.LogInformation(message: "Queue {QueueName} declared", args: queueName);
    }

    public void BindQueue(string queueName, string exchangeName)
    {
        _model.QueueBind(
            queue: queueName,
            exchange: exchangeName,
            routingKey: queueName);
        _logger.LogInformation("Queue {QueueName} binded to exchange {ExchangeName}", queueName, exchangeName);
    }

    public void Dispose()
    {
        _model?.Dispose();
        _model = null;
        _connection?.Dispose();
        _connection = null;
    }
}