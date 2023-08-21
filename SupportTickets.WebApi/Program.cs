using System.Reflection;
using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;
using SupportTickets.WebApi.Constants;
using SupportTickets.WebApi.Filters;
using SupportTickets.WebApi.Repositories.Messages;
using SupportTickets.WebApi.Repositories.Solutions;
using SupportTickets.WebApi.Repositories.SupportTicketAgentRecords;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.SupportTicketStatusRecords;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services;
using SupportTickets.WebApi.Services.JobsManagers.Closing;
using SupportTickets.WebApi.Services.JobsManagers.Escalations;
using SupportTickets.WebApi.Services.Users;

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

    builder.Services
        .AddRabbitMqMessageConsumer(builder.Configuration.GetRequiredSection("RabbitMqOptions"))
        .AddAutoMapper(typeof(Program))
        .AddTransient<IUsersService, UsersService>()
        .AddHostedService<RabbitMqWorker>()
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