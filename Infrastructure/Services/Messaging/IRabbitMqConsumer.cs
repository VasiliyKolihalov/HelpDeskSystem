namespace Infrastructure.Services.Messaging;

public interface IRabbitMqConsumer
{
    public void Start(Dictionary<string, Func<string, Task>> consumers);
    public void Stop();
}