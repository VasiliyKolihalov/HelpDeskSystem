using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Api.Models.Options;
using Notification.Api.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .AddOptionsWithDataAnnotationsValidation<EmailOptions>(
                context.Configuration.GetRequiredSection("EmailOptions"))
            .AddTransient<IEmailService, EmailService>()
            .AddTransient<INotificationsService, NotificationsService>()
            .AddRabbitMqMessageConsumer(context.Configuration.GetRequiredSection("RabbitMqOptions"))
            .AddHostedService<RabbitMqWorker>();
    })
    .Build()
    .RunAsync();