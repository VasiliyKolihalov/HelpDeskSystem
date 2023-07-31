using System.Text;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Services.Messaging;

public sealed class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly List<string> _consumerTags = new();

    private IConnection? _connection;
    private IModel? _model;

    public RabbitMqConsumer(
        ConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        ILogger<RabbitMqConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
    }

    public void Start(Dictionary<string, Func<string, Task>> consumers)
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
                _connectionFactory.DispatchConsumersAsync = true;
                _connection = _connectionFactory.CreateConnection();
                _model = _connection.CreateModel();

                foreach (KeyValuePair<string, Func<string, Task>> keyValuePair in consumers)
                {
                    ConsumeQueue(queueName: keyValuePair.Key, consumer: keyValuePair.Value);
                }
            });
    }

    private void ConsumeQueue(string queueName, Func<string, Task> consumer)
    {
        var asyncEventingBasicConsumer = new AsyncEventingBasicConsumer(_model);
        asyncEventingBasicConsumer.Received += async (_, eventArgs) =>
        {
            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            await consumer.Invoke(message);
        };

        string tag = _model.BasicConsume(
            queue: queueName,
            autoAck: true,
            consumer: asyncEventingBasicConsumer);
        _consumerTags.Add(tag);
    }

    public void Stop()
    {
        foreach (string tag in _consumerTags)
        {
            _model?.BasicCancel(tag);
        }

        _model?.Close();
        _connection?.Close();
    }

    public void Dispose()
    {
        _model?.Dispose();
        _model = null;
        _connection?.Dispose();
        _connection = null;
    }
}