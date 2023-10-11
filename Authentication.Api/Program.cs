using System.Reflection;
using Infrastructure.Authentication.Extensions;
using Infrastructure.Authentication.Models;
using Infrastructure.Authentication.Services;
using Authentication.Api.Constants;
using Authentication.Api.Repositories.Accounts;
using Authentication.Api.Repositories.EmailConfirmCodes;
using Authentication.Api.Repositories.Permissions;
using Authentication.Api.Repositories.Roles;
using Authentication.Api.Services;
using Authentication.Api.Services.Clients.Users;
using Authentication.Api.Services.EmailConfirmCodes;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Infrastructure.Services.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);
WebApplication app = builder.Build();
await app.UseFluentMigrationAsync(async options => await options.CreateDatabaseAsync("AccountsDb"));
ConfigureMiddlewares(app);
app.Run();
return;

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
        .AddTransient<IEmailConfirmCodesRepository, EmailConfirmCodesRepository>()
        .AddTransient<IPermissionsRepository, PermissionsRepository>();
    
    builder.Services
        .AddAutoMapper(typeof(Program))
        .AddRabbitMqMessagePublisher(builder.Configuration.GetRequiredSection("RabbitMqOptions"))
        .AddTransient<IJwtService, JwtService>()
        .AddTransient<IUsersClient, UsersClient>()
        .AddTransient<IEmailConfirmCodeProvider, EmailConfirmCodeProvider>()
        .AddTransient<AccountsService>()
        .AddTransient<RolesService>()
        .AddTransient<PermissionsService>()
        .AddHttpClient(
            name: HttpClientNames.Users,
            configureClient: client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration.GetRequiredValue<string>("HttpUrls:Users.Api"));
            })
        .AddDefaultRetryPollyHandler(builder.Configuration.GetRequiredSection("PollyOptions"));


    IConfigurationSection jwtAuthSection = builder.Configuration.GetRequiredSection("JwtAuthOptions");
    builder.Services.Configure<JwtAuthOptions>(jwtAuthSection);

    builder.Services
        .AddJwtAuthentication(jwtAuthSection)
        .AddAuthorization(options => options
            .AddPoliticiansBasedOnJwtPermissions(PermissionNames.AllPolicyPermissions));

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