using System.Reflection;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddAutoMapper(typeof(Program))
    .AddRabbitMqMessageConsumer("RabbitMqConfiguration")
    .AddHostedService<UsersWorker>();

string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? throw new InvalidOperationException("Default connection string is required");
string masterConnectionString = builder.Configuration.GetConnectionString("Master")
                                ?? throw new InvalidOperationException("Master connection string is required");

builder.Services.AddSingleton<IDbContext, PostgresContext>(_ => new PostgresContext(
    connectionString: connectionString,
    masterConnectionString: masterConnectionString));

builder.Services
    .AddTransient<ISupportTicketsRepository, SupportTicketsRepository>()
    .AddTransient<IUsersRepository, UsersRepository>();

builder.Services.AddTransient<SupportTicketsService>();

builder.Services.AddFluentMigrationForPostgres(connectionString, Assembly.GetExecutingAssembly());

var app = builder.Build();

await app.UseFluentMigrationAsync(async options => await options.CreateDatabaseAsync("supportticketsdb"));

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();