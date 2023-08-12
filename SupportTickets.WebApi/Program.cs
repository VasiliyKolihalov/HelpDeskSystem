using System.Reflection;
using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Constants;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services;
using SupportTickets.WebApi.Services.Clients;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);
WebApplication app = builder.Build();
await app.UseFluentMigrationAsync(async options => await options.CreateDatabaseAsync("SupportTicketsDb"));
ConfigureMiddlewares(app);
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
        .AddTransient<IUsersService, UsersService>()
        .AddTransient<IJwtService, JwtService>()
        .AddTransient<IAccountsClient, AccountsClient>()
        .AddHostedService<RabbitMqWorker>()
        .AddTransient<SupportTicketsService>()
        .AddHttpClient(
            name: HttpClientNames.Accounts,
            configureClient: client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration.GetRequiredValue<string>("HttpUrls:Accounts.WebApi"));
            })
        .AddDefaultRetryPollyHandler(builder.Configuration.GetRequiredSection("PollyOptions"));

    IConfigurationSection jwtAuthSection = builder.Configuration.GetRequiredSection("JwtAuthOptions");
    builder.Services.Configure<JwtAuthOptions>(jwtAuthSection);

    builder.Services
        .AddJwtAuthentication(jwtAuthSection)
        .AddAuthorization(options =>
        {
            options
                .AddPoliticiansBasedOnJwtPermissions(PermissionNames.AllPolicyPermissions)
                .AddEmailConfirmPolicy();
        });

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(opt => opt.AddJwtSecurity())
        .AddControllers();
}

static void ConfigureMiddlewares(WebApplication app)
{
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