using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddRabbitMqMessageConsumer("RabbitMqConfiguration");
builder.Services.AddHostedService<UsersWorker>();

string connectionString = builder.Configuration.GetConnectionString("Default")!;
RegisterRepositories(builder.Services, connectionString);
builder.Services.AddTransient<SupportTicketsService>();

var app = builder.Build();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();
app.Run();

static void RegisterRepositories(IServiceCollection serviceCollection, string connectionString)
{
    serviceCollection
        .AddTransient<ISupportTicketsRepository>(_ => new SupportTicketsRepository(connectionString))
        .AddTransient<IUsersRepository>(_ => new UsersRepository(connectionString));
}