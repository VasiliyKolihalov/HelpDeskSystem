using System.Reflection;
using Authentication.Infrastructure.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Constants;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);

WebApplication app = builder.Build();
await ConfigureMiddlewaresAsync(app);

app.Run();

static void ConfigureServices(WebApplicationBuilder builder)
{
    string connectionString = builder.Configuration.GetRequiredConnectionString("Default");
    builder.Services
        .AddSingleton<IDbContext, PostgresContext>(_ => new PostgresContext(
            connectionString: connectionString,
            masterConnectionString: builder.Configuration.GetRequiredConnectionString("Master")))
        .AddFluentMigrationForPostgres(connectionString, Assembly.GetExecutingAssembly())
        .AddTransient<ISupportTicketsRepository, SupportTicketsRepository>()
        .AddTransient<IUsersRepository, UsersRepository>();

    builder.Services
        .AddRabbitMqMessageConsumer(builder.Configuration.GetRequiredSection("RabbitMqOptions"))
        .AddAutoMapper(typeof(Program))
        .AddHostedService<UsersWorker>()
        .AddTransient<SupportTicketsService>();

    builder.Services
        .AddJwtAuthentication(builder.Configuration.GetRequiredSection("JwtAuthOptions"))
        .AddAuthorization(options => options
            .AddPolicyBasedOnJwtPermissions(PermissionsConstants.AllPermissionsForPolicy));

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(opt => opt.AddJwtSecurity())
        .AddControllers();
}

static async Task ConfigureMiddlewaresAsync(WebApplication app)
{
    await app.UseFluentMigrationAsync(async options => await options.CreateDatabaseAsync("SupportTicketsDb"));

    app.UseHttpLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}