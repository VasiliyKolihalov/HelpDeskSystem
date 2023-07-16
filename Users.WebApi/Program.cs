using System.Reflection;
using FluentMigrator.Runner;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;
using Users.WebApi.Repositories;
using Users.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddAutoMapper(typeof(Program))
    .AddRabbitMqMessagePublisher("RabbitMqConfiguration");

string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? throw new InvalidOperationException("Default connection string is required");
string masterConnectionString = builder.Configuration.GetConnectionString("Master")
                                ?? throw new InvalidOperationException("Master connection string is required");

builder.Services.AddSingleton<IDbContext, PostgresContext>(_ => new PostgresContext(
    connectionString: connectionString,
    masterConnectionString: masterConnectionString));

builder.Services.AddFluentMigrationForPostgres(connectionString, Assembly.GetExecutingAssembly());

builder.Services.AddTransient<IUsersRepository, UsersRepository>();
builder.Services.AddTransient<UsersService>();

var app = builder.Build();

await app.UseFluentMigrationAsync(async options => await options.CreateDatabaseAsync("usersdb"));

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();