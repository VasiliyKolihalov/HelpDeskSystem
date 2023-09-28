using System.Reflection;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Authentication.Extensions;
using Infrastructure.Authentication.Models;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;
using SupportTickets.Api.Constants;
using SupportTickets.Api.Filters;
using SupportTickets.Api.Repositories.Messages;
using SupportTickets.Api.Repositories.Solutions;
using SupportTickets.Api.Repositories.SupportTicketAgentRecords;
using SupportTickets.Api.Repositories.SupportTickets;
using SupportTickets.Api.Repositories.SupportTicketStatusRecords;
using SupportTickets.Api.Repositories.Users;
using SupportTickets.Api.Services;
using SupportTickets.Api.Services.Clients;
using SupportTickets.Api.Services.JobsManagers.Closing;
using SupportTickets.Api.Services.JobsManagers.Escalations;
using SupportTickets.Api.Services.Users;

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
        .AddTransient<IMessagesRepository, MessagesRepository>()
        .AddTransient<ISolutionsRepository, SolutionsRepository>()
        .AddTransient<ISupportTicketAgentRecordsRepository, SupportTicketAgentRecordsRepository>()
        .AddTransient<ISupportTicketStatusRecordsRepository, SupportTicketStatusRecordsRepository>()
        .AddTransient<IUsersRepository, UsersRepository>();

    builder.Services.AddHangfire(options => { options.UsePostgreSqlStorage(connectionString); });
    builder.Services.AddHangfireServer();

    builder.Services.AddGrpcClient<Images.ImagesClient>(
            name: GrpcClientsNames.Images,
            configureClient: client =>
            {
                client.Address = new Uri(builder.Configuration.GetRequiredValue<string>("GrpcUrls:Resources"));
            })
        .AddDefaultRetryPollyHandler(builder.Configuration.GetRequiredSection("PollyOptions"));

    builder.Services
        .AddRabbitMqMessageConsumer(builder.Configuration.GetRequiredSection("RabbitMqOptions"))
        .AddAutoMapper(typeof(Program))
        .AddTransient<IUsersService, UsersService>()
        .AddHostedService<RabbitMqWorker>()
        .AddTransient<IResourcesGrpcClient, ResourcesGrpcClient>()
        .AddTransient<ISupportTicketsEscalationManager, SupportTicketsEscalationManager>()
        .AddTransient<ISupportTicketsClosingManager, SupportTicketsClosingManager>()
        .AddTransient<SupportTicketsService>();

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

    app.UseHangfireDashboard(options: new DashboardOptions
    {
        Authorization = new[] { new DashboardAuthFilter() }
    });
}