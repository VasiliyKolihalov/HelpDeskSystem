using System.Reflection;
using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Authentication.Infrastructure.Services;
using Authentication.WebApi.Clients.Users;
using Authentication.WebApi.Constants;
using Authentication.WebApi.Repositories;
using Authentication.WebApi.Services;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Models;
using Infrastructure.Services.Persistence;

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
        .AddTransient<IAccountsRepository, AccountsRepository>()
        .AddTransient<IRolesRepository, RolesRepository>()
        .AddTransient<IPermissionsRepository, PermissionsRepository>();

    builder.Services
        .AddOptions<PollyOptions>()
        .Bind(builder.Configuration.GetRequiredSection("PollyOptions"))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services
        .AddAutoMapper(typeof(Program))
        .AddTransient<IJwtService, JwtService>()
        .AddTransient<IUsersClient, UsersClient>()
        .AddTransient<AccountsService>()
        .AddTransient<RolesService>()
        .AddTransient<PermissionsService>()
        .AddHttpClient(
            name: HttpClientNames.Users,
            configureClient: client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration
                        .GetRequiredSection("HttpUrls")
                        .GetRequiredValue<string>("Users.WebApi"));
            });


    IConfigurationSection jwtAuthSection = builder.Configuration.GetRequiredSection("JwtAuthOptions");
    builder.Services.Configure<JwtAuthOptions>(jwtAuthSection);

    builder.Services
        .AddJwtAuthentication(jwtAuthSection)
        .AddAuthorization(options => options
            .AddPolicyBasedOnJwtPermissions(PermissionNames.AllPermissionsForPolicy));

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(opt => opt.AddJwtSecurity())
        .AddControllers();
}

static async Task ConfigureMiddlewaresAsync(WebApplication app)
{
    Console.WriteLine("хуй");
    await app.UseFluentMigrationAsync(async options => await options.CreateDatabaseAsync("AccountsDb"));

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