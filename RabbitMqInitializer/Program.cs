using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMqInitializer;


using RabbitMqDeclarator declarator = CreateRabbitMqDeclarator();
declarator.Connect();

DeclareExchanges(declarator);
DeclareQueues(declarator);
BindQueues(declarator);

return;

static RabbitMqDeclarator CreateRabbitMqDeclarator()
{
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
    var rabbitMqOptions = configuration.GetSection("RabbitMqOptions").GetAndValidate<RabbitMqOptions>();
    using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
    ILogger logger = loggerFactory.CreateLogger<Program>();

    return new RabbitMqDeclarator(rabbitMqOptions, logger);
}

static void DeclareExchanges(RabbitMqDeclarator declarator)
{
    declarator.DeclareExchange("users");
    declarator.DeclareExchange("notifications");
}

static void DeclareQueues(RabbitMqDeclarator declarator)
{
    declarator.DeclareQueue("users.created");
    declarator.DeclareQueue("users.deleted");
    declarator.DeclareQueue("users.updated");
    declarator.DeclareQueue("notifications.requested_email_confirm_code");
}

static void BindQueues(RabbitMqDeclarator declarator)
{
    declarator.BindQueue(queueName: "users.created", exchangeName: "users");
    declarator.BindQueue(queueName: "users.deleted", exchangeName: "users");
    declarator.BindQueue(queueName: "users.updated", exchangeName: "users");
    declarator.BindQueue(queueName: "notifications.requested_email_confirm_code", exchangeName: "notifications");
}