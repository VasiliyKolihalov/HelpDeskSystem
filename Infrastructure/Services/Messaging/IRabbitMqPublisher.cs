namespace Infrastructure.Services.Messaging;

public interface IRabbitMqPublisher
{
    public void PublishMessage(object message, string routingKey);
}