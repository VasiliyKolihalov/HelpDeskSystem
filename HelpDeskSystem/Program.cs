using HelpDeskSystem.Constants;
using HelpDeskSystem.Extensions;
using HelpDeskSystem.Middlewares;
using HelpDeskSystem.Models;
using HelpDeskSystem.Repositories;
using HelpDeskSystem.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);
WebApplication app = builder.Build();
RegisterMiddlewares(app);
app.Run();

static void RegisterServices(WebApplicationBuilder builder)
{
    string connectionString = builder.Configuration.GetConnectionString("Default");
    builder.Services.AddTransient<IApplicationContext>(_ => new ApplicationContext(connectionString));

    IConfigurationSection jwtAuthOptions = builder.Configuration.GetSection("Jwt");
    builder.Services.Configure<JwtAuthOptions>(jwtAuthOptions);
    builder.Services.AddJwtAuthentication(jwtAuthOptions.Get<JwtAuthOptions>());

    builder.Services.AddAuthorization(options =>
    {
        options.AddPoliticiansBasedOnPermissions(PermissionNames.AllPermissions, CustomClaimTypes.Permission);
    });

    builder.Services
        .AddTransient<IJwtService, JwtService>()
        .AddTransient<SupportTicketsService>()
        .AddTransient<AccountsService>()
        .AddTransient<UsersService>()
        .AddTransient<RolesService>();

    builder.Services
        .AddAutoMapper(typeof(Program))
        .AddControllers();
}

static void RegisterMiddlewares(WebApplication app)
{
    app
        .UseAuthentication()
        .UseAuthorization()
        .UseHttpLogging()
        .UseMiddleware<ErrorHandlerMiddleware>();

    app.MapControllers();
}