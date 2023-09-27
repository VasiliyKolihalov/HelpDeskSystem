using System.Text;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;

namespace Infrastructure.Services.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly ConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqPublisher> _logger;

    private IConnection? _connection;
    private IModel? _model;

    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> rabbitMqOptions,
        ConnectionFactory connectionFactory,
        ILogger<RabbitMqPublisher> logger)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void PublishMessage(object message, string routingKey)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));

        string data = JsonConvert.SerializeObject(message);
        byte[] body = Encoding.UTF8.GetBytes(data);
        _model.BasicPublish(
            exchange: _rabbitMqOptions.ExchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
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

    public void Dispose()
    {
        _model?.Dispose();
        _model = null;
        _connection?.Dispose();
        _connection = null;
    }
}